using System;

namespace ZXTemplate.UI
{
    /// <summary>
    /// High-level API to show a confirm dialog.
    ///
    /// Semantics:
    /// - onConfirm: user clicks the confirm button (e.g. "OK", "Keep", "Yes")
    /// - onCancel: user clicks cancel OR the dialog times out (when timeoutSeconds > 0)
    ///
    /// timeoutAsCancel:
    /// - true: timeout triggers onCancel
    /// - false: timeout triggers onConfirm
    /// </summary>
    public interface IConfirmService
    {
        void Show(
            string title,
            string message,
            string confirmText,
            string cancelText,
            Action onConfirm,
            Action onCancel,
            float timeoutSeconds = 0f,
            bool timeoutAsCancel = true);
    }
}
