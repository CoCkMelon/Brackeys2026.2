using TMPro;
using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// Visual prefab for toast messages.
    ///
    /// Requirements / conventions:
    /// - Should be spawned on UIOverlay channel "Toast".
    /// - Should NOT block input (CanvasGroup.blocksRaycasts = false on prefab recommended).
    /// - Alpha is controlled by ToastService (fade-in/out).
    /// </summary>
    public class ToastView : UIWindow
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageText;

        /// <summary>Sets the toast message text.</summary>
        public void SetMessage(string msg)
        {
            if (messageText) messageText.text = msg ?? "";
        }

        /// <summary>Sets current alpha (0..1) for fade animation.</summary>
        public void SetAlpha(float a)
        {
            if (canvasGroup) canvasGroup.alpha = a;
        }

        private void Reset()
        {
            // Auto-wire when component is added in editor.
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
