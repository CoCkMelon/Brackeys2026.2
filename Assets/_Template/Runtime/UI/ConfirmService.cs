using System;

namespace ZXTemplate.UI
{
    public class ConfirmService : IConfirmService
    {
        private readonly IUIService _ui;
        private readonly ConfirmDialogWindow _prefab;

        public ConfirmService(IUIService ui, ConfirmDialogWindow prefab)
        {
            _ui = ui;
            _prefab = prefab;
        }

        public void Show(string title, string message, string confirmText, string cancelText,
            Action onConfirm, Action onCancel,
            float timeoutSeconds = 0f, bool timeoutAsCancel = true)
        {
            var win = (ConfirmDialogWindow)_ui.Push(_prefab);
            win.Setup(title, message, confirmText, cancelText, onConfirm, onCancel, timeoutSeconds, timeoutAsCancel);
        }
    }
}
