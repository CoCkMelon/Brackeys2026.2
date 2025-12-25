using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// The persistent UI root object.
    ///
    /// It groups all UI subsystems so they can be kept across scenes:
    /// - Canvas: the main Unity UI canvas (and typically the EventSystem exists under this root)
    /// - UIStack: modal windows stack (Settings, Pause, MainMenu, etc.)
    /// - UIOverlay: non-stacked overlays (HUD, Toast, etc.) with channels
    /// - LoadingScreen: a simple loading indicator that can be toggled by UIService
    ///
    /// Template conventions:
    /// - UIRoot should exist only once (created by Bootstrapper and marked DontDestroyOnLoad).
    /// - Overlay root RectTransform should be full screen (stretch to parent),
    ///   otherwise overlays will appear "centered inside a small box".
    /// </summary>
    public class UIRoot : MonoBehaviour
    {
        public Canvas Canvas => _canvas;
        public UIStack Stack => _stack;
        public UIOverlay Overlay => _overlay;
        public LoadingScreen Loading => _loading;

        [Header("References")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private UIStack _stack;
        [SerializeField] private UIOverlay _overlay;
        [SerializeField] private LoadingScreen _loading;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Editor-only safety checks (help catch prefab wiring mistakes early).
            if (_canvas == null) Debug.LogWarning("[UIRoot] Canvas reference is missing.", this);
            if (_stack == null) Debug.LogWarning("[UIRoot] UIStack reference is missing.", this);
            if (_overlay == null) Debug.LogWarning("[UIRoot] UIOverlay reference is missing.", this);
            if (_loading == null) Debug.LogWarning("[UIRoot] LoadingScreen reference is missing.", this);
        }
#endif
    }
}
