using TMPro;
using UnityEngine;

namespace ZXTemplate.UI
{
    public class ToastView : UIWindow
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageText;

        public void SetMessage(string msg)
        {
            if (messageText) messageText.text = msg ?? "";
        }

        public void SetAlpha(float a)
        {
            if (canvasGroup) canvasGroup.alpha = a;
        }

        private void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
