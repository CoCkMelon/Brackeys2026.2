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
    public class Bootstrapper : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIRoot uiRootPrefab;
        [SerializeField] private ConfirmDialogWindow confirmDialogPrefab;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Audio")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioLibrary audioLibrary;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;


        private void Awake()
        {
            // 防止重复 Bootstrap
            var all = FindObjectsByType<Bootstrapper>(FindObjectsSortMode.None);
            if (all.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            // 1) UI Root（常驻）
            var uiRoot = Instantiate(uiRootPrefab);
            DontDestroyOnLoad(uiRoot.gameObject);

            var uiService = new UIService(uiRoot);
            ServiceContainer.Register<IUIService>(uiService);

            var confirmService = new ConfirmService(uiService, confirmDialogPrefab);
            ServiceContainer.Register<IConfirmService>(confirmService);


            // 2) Input
            var inputService = new InputService(inputActions);
            ServiceContainer.Register<IInputService>(inputService);

            // Input Mode Stack（你上一阶段做的）
            var inputModeService = new InputModeService(inputService);
            ServiceContainer.Register<IInputModeService>(inputModeService);

            // 3) Pause
            var pauseService = new PauseService();
            ServiceContainer.Register<IPauseService>(pauseService);

            // 4) Audio
            // 避免未来 timeScale=0 时 mixer 更新冻结（安全项）
            if (audioMixer != null)
                audioMixer.updateMode = AudioMixerUpdateMode.UnscaledTime;

            var audioService = new AudioService(audioMixer, audioLibrary, bgmGroup, sfxGroup);
            ServiceContainer.Register<IAudioService>(audioService);

            // 5) Save (JSON)
            var saveService = new JsonSaveService();
            ServiceContainer.Register<ISaveService>(saveService);

            // 6) Settings（统一 settings_main，未来扩展 video/language/controls）
            var settingsService = new SettingsService(saveService);
            ServiceContainer.Register<ISettingsService>(settingsService);

            // 7) Progress（最小版：coins/highScore/unlockedLevel + 自定义 ints）
            var progressService = new ProgressService(saveService);
            ServiceContainer.Register<IProgressService>(progressService);

            // 8) SaveManager（统一保存：Settings + Progress）
            var saveManager = new SaveManager();
            saveManager.Register(settingsService);
            saveManager.Register(progressService);
            ServiceContainer.Register<ISaveManager>(saveManager);

            // 让 SaveManager 接入 Unity 生命周期（退出/切后台自动 SaveAll）
            var saveRunnerGo = new GameObject("@SaveManager");
            DontDestroyOnLoad(saveRunnerGo);
            var runner = saveRunnerGo.AddComponent<SaveManagerRunner>();
            runner.Initialize(saveManager);

            // 9) Scene Service（最后注册，加载场景会清 UI）
            var sceneService = new SceneService(uiService);
            ServiceContainer.Register<ISceneService>(sceneService);

            // 10) 启动时应用一次设置（重要：一进游戏就按 settings 生效）
            settingsService.ApplyAll();

        }

        private void OnDestroy()
        {
            // optional: ServiceContainer.Clear();
        }
    }
}
