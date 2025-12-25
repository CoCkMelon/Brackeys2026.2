using System;

namespace ZXTemplate.UI
{
    public interface IConfirmService
    {
        /// <summary>
        /// Show a confirm dialog.
        /// onConfirm = user clicks confirm
        /// onCancel  = user clicks cancel OR auto-timeout (if timeoutSeconds > 0)
        /// </summary>
        void Show(string title, string message, string confirmText, string cancelText,
                  Action onConfirm, Action onCancel,
                  float timeoutSeconds = 0f, bool timeoutAsCancel = true);
    }
}
