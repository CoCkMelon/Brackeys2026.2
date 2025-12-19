using UnityEngine;

namespace ZXTemplate.UI
{
    public class UIRoot : MonoBehaviour
    {
        public Canvas Canvas => _canvas;
        public UIStack Stack => _stack;
        public UIOverlay Overlay => _overlay;
        public LoadingScreen Loading => _loading;

        [SerializeField] private Canvas _canvas;
        [SerializeField] private UIStack _stack;
        [SerializeField] private UIOverlay _overlay;
        [SerializeField] private LoadingScreen _loading;
    }
}
