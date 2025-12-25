using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// A simple loading screen view controlled by UIService / SceneService.
    ///
    /// Implementation:
    /// - Uses CanvasGroup so we can toggle:
    ///   - alpha (visibility)
    ///   - blocksRaycasts (prevent clicking UI behind the loading screen)
    ///   - interactable (optional)
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;

        /// <summary>Shows the loading screen and blocks input behind it.</summary>
        public void Show()
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
            group.interactable = true;
        }

        /// <summary>Hides the loading screen and restores input to underlying UI.</summary>
        public void Hide()
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            group = GetComponent<CanvasGroup>();
        }
#endif
    }
}
