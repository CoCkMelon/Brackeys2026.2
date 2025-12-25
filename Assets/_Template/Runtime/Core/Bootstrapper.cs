using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using ZXTemplate.Audio;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Progress;
using ZXTemplate.Save;
using ZXTemplate.Scenes;
using ZXTemplate.Settings;
using ZXTemplate.UI;

namespace ZXTemplate.Core
{
    /// <summary>
    /// Bootstrapper is the single entry point of the template.
    ///
    /// Responsibilities:
    /// - Create persistent runtime services (UI/Input/Audio/Save/Settings/Progress/Scene).
    /// - Register services into ServiceContainer for global access.
    /// - Ensure there is only ONE bootstrapper instance across scene loads.
    ///
    /// Scene usage:
    /// - Put this component in the "Bootstrap" scene only.
    /// - Always start the game from Bootstrap scene (or add a redirect script in Editor).
    ///
    /// Notes:
    /// - We keep UI Root and service runners alive using DontDestroyOnLoad.
    /// - Settings.ApplyAll() is called once here so audio/video/controls are correct immediately.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIRoot uiRootPrefab;
        [SerializeField] private ConfirmDialogWindow confirmDialogPrefab;
        [SerializeField] private ToastView toastPrefab;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioLibrary audioLibrary;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        private void Awake()
        {
            // ------------------------------------------------------------
            // 0) Ensure singleton bootstrapper (avoid duplicated services)
            // ------------------------------------------------------------
            var all = FindObjectsByType<Bootstrapper>(FindObjectsSortMode.None);
            if (all.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            // ------------------------------------------------------------
            // 1) UI Root (persistent)
            // - Holds Canvas, EventSystem, UIStack, and Overlay roots.
            // ------------------------------------------------------------
            var uiRoot = Instantiate(uiRootPrefab);
            DontDestroyOnLoad(uiRoot.gameObject);

            var uiService = new UIService(uiRoot);
            ServiceContainer.Register<IUIService>(uiService);

            // Confirm dialog (used by video safe revert, reset bindings, etc.)
            var confirmService = new ConfirmService(uiService, confirmDialogPrefab);
            ServiceContainer.Register<IConfirmService>(confirmService);

            // Toast notifications (small non-blocking messages)
            var toastService = new ToastService(uiService, toastPrefab);
            ServiceContainer.Register<IToastService>(toastService);

            // ------------------------------------------------------------
            // 2) Input
            // - InputService owns the InputActionAsset reference and input queries.
            // - InputModeService controls which action maps are enabled (UI vs Gameplay).
            // ------------------------------------------------------------
            var inputService = new InputService(inputActions);
            ServiceContainer.Register<IInputService>(inputService);

            var inputModeService = new InputModeService(inputService);
            ServiceContainer.Register<IInputModeService>(inputModeService);

            // ------------------------------------------------------------
            // 3) Pause
            // - A central pause state manager (tokens/stack-based if you extended it).
            // ------------------------------------------------------------
            var pauseService = new PauseService();
            ServiceContainer.Register<IPauseService>(pauseService);

            // ------------------------------------------------------------
            // 4) Audio
            // - Use UnscaledTime so mixer updates still work when timeScale = 0.
            // ------------------------------------------------------------
            if (audioMixer != null)
                audioMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;

            var audioService = new AudioService(audioMixer, audioLibrary, bgmGroup, sfxGroup);
            ServiceContainer.Register<IAudioService>(audioService);

            // ------------------------------------------------------------
            // 5) Save (JSON)
            // - Low-level persistence: save/load by key.
            // ------------------------------------------------------------
            var saveService = new JsonSaveService();
            ServiceContainer.Register<ISaveService>(saveService);

            // ------------------------------------------------------------
            // 6) Settings
            // - Persistent user preferences: audio/video/controls (and future options).
            // ------------------------------------------------------------
            var settingsService = new SettingsService(saveService);
            ServiceContainer.Register<ISettingsService>(settingsService);

            // ------------------------------------------------------------
            // 7) Progress
            // - Minimal player progress data (coins/high score/unlocks/custom ints).
            // ------------------------------------------------------------
            var progressService = new ProgressService(saveService);
            ServiceContainer.Register<IProgressService>(progressService);

            // ------------------------------------------------------------
            // 8) SaveManager
            // - Central coordinator that calls Save() on participants.
            // - We also attach a runner MonoBehaviour to handle Unity lifecycle events.
            // ------------------------------------------------------------
            var saveManager = new SaveManager();
            saveManager.Register(settingsService);
            saveManager.Register(progressService);
            ServiceContainer.Register<ISaveManager>(saveManager);

            var saveRunnerGo = new GameObject("@SaveManager");
            DontDestroyOnLoad(saveRunnerGo);
            var runner = saveRunnerGo.AddComponent<SaveManagerRunner>();
            runner.Initialize(saveManager);

            // ------------------------------------------------------------
            // 9) Scene Service (registered last)
            // - Handles scene transitions (often clears UI stack / overlays).
            // ------------------------------------------------------------
            var sceneService = new SceneService(uiService);
            ServiceContainer.Register<ISceneService>(sceneService);

            // ------------------------------------------------------------
            // 10) Apply settings once at startup
            // - Make sure volume, resolution, bindings are correct immediately.
            // ------------------------------------------------------------
            settingsService.ApplyAll();
        }

        private void OnDestroy()
        {
            // Usually not needed. Only clear if you implement a full reset flow.
            // ServiceContainer.Clear();
        }
    }
}
