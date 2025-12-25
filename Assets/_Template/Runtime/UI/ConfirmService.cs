using System;

namespace ZXTemplate.UI
{
    /// <summary>
    /// High-level API to show a confirm dialog.
    ///
    /// Responsibilities:
    /// - Push a ConfirmDialogWindow onto the UIStack.
    /// - Pass title/message/button labels and callbacks to the window.
    ///
    /// Notes:
    /// - The dialog itself handles button clicks and optional timeout.
    /// - Since it is pushed onto UIStack, it will behave like a modal window
    ///   (blocks interaction with windows behind it).
    /// </summary>
    public class ConfirmService : IConfirmService
    {
        private readonly IUIService _ui;
        private readonly ConfirmDialogWindow _prefab;

        public ConfirmService(IUIService ui, ConfirmDialogWindow prefab)
        {
            _ui = ui;
            _prefab = prefab;
        }

        /// <summary>
        /// Shows a confirm dialog.
        ///
        /// timeoutSeconds:
        /// - 0 or negative: no timeout
        /// - positive: dialog counts down using unscaled time (handled in ConfirmDialogWindow)
        ///
        /// timeoutAsCancel:
        /// - true: timeout triggers Cancel callback
        /// - false: timeout triggers Confirm callback
        /// </summary>
        public void Show(
            string title,
            string message,
            string confirmText,
            string cancelText,
            Action onConfirm,
            Action onCancel,
            float timeoutSeconds = 0f,
            bool timeoutAsCancel = true)
        {
            var win = (ConfirmDialogWindow)_ui.Push(_prefab);
            win.Setup(title, message, confirmText, cancelText, onConfirm, onCancel, timeoutSeconds, timeoutAsCancel);
        }
    }
}
