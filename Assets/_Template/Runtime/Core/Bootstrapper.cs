using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Scenes;
using ZXTemplate.UI;
using ZXTemplate.Input;
using ZXTemplate.Save;
using ZXTemplate.Audio;

namespace ZXTemplate.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private UIRoot uiRootPrefab;

        [Header("Input")]
        [SerializeField] private UnityEngine.InputSystem.InputActionAsset inputActions;

        [Header("Audio")]
        [SerializeField] private UnityEngine.Audio.AudioMixer audioMixer;
        [SerializeField] private AudioLibrary audioLibrary;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            // 1) UI
            var uiRoot = Instantiate(uiRootPrefab);
            DontDestroyOnLoad(uiRoot.gameObject);
            var uiService = new UIService(uiRoot);

            // 2) Core services
            var pauseService = new PauseService();
            var sceneService = new SceneService(uiService);
            var inputService = new InputService(inputActions);
            var saveService = new JsonSaveService();

            // 3) Audio service (depends on UI? no)
            var audioService = new AudioService(audioMixer, audioLibrary);

            // Register
            ServiceContainer.Register<IUIService>(uiService);
            ServiceContainer.Register<IPauseService>(pauseService);
            ServiceContainer.Register<ISceneService>(sceneService);
            ServiceContainer.Register<IInputService>(inputService);
            ServiceContainer.Register<ISaveService>(saveService);
            ServiceContainer.Register<IAudioService>(audioService);
        }

        private void OnDestroy()
        {
            // optional: ServiceContainer.Clear();
        }
    }
}
