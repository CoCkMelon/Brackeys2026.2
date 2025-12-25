using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ZXTemplate.UI
{
    /// <summary>
    /// A reusable confirm dialog window.
    ///
    /// Features:
    /// - Title + message
    /// - Confirm / Cancel buttons with custom labels
    /// - Optional timeout (uses unscaled time so it still counts down during pause)
    ///
    /// Usage pattern:
    /// - Created and pushed by ConfirmService.
    /// - Caller provides onConfirm/onCancel actions.
    /// - This window pops itself from the UI stack after an action.
    /// </summary>
    public class ConfirmDialogWindow : UIWindow
    {
        [Header("UI")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private TMP_Text timerText;

        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        [SerializeField] private TMP_Text confirmButtonText;
        [SerializeField] private TMP_Text cancelButtonText;

        private Action _onConfirm;
        private Action _onCancel;

        private float _timeout;
        private bool _timeoutAsCancel;
        private bool _running;

        /// <summary>
        /// Initializes dialog contents and callbacks.
        /// timeoutSeconds <= 0 disables timeout.
        /// timeoutAsCancel = true means timeout triggers Cancel; otherwise triggers Confirm.
        /// </summary>
        public void Setup(
            string title, string message,
            string confirmText, string cancelText,
            Action onConfirm, Action onCancel,
            float timeoutSeconds, bool timeoutAsCancel)
        {
            if (titleText) titleText.text = title ?? "";
            if (messageText) messageText.text = message ?? "";

            if (confirmButtonText) confirmButtonText.text = string.IsNullOrEmpty(confirmText) ? "OK" : confirmText;
            if (cancelButtonText) cancelButtonText.text = string.IsNullOrEmpty(cancelText) ? "Cancel" : cancelText;

            _onConfirm = onConfirm;
            _onCancel = onCancel;

            _timeout = Mathf.Max(0f, timeoutSeconds);
            _timeoutAsCancel = timeoutAsCancel;

            UpdateTimerLabel(_timeout);
        }

        public override void OnPushed()
        {
            _running = true;

            // Bind UI events here (instead of OnEnable) so UIStack hide/show is safe.
            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        public override void OnPopped()
        {
            _running = false;

            // Always unbind to avoid leaking listeners.
            confirmButton.onClick.RemoveListener(Confirm);
            cancelButton.onClick.RemoveListener(Cancel);
        }

        private void Update()
        {
            if (!_running) return;
            if (_timeout <= 0f) return;

            // Use unscaled time so countdown still works when game is paused (Time.timeScale = 0).
            _timeout -= Time.unscaledDeltaTime;
            UpdateTimerLabel(_timeout);

            if (_timeout <= 0f)
            {
                if (_timeoutAsCancel) Cancel();
                else Confirm();
            }
        }

        private void UpdateTimerLabel(float secondsLeft)
        {
            if (!timerText) return;

            if (secondsLeft > 0f)
                timerText.text = $"({Mathf.CeilToInt(secondsLeft)})";
            else
                timerText.text = "";
        }

        private void Confirm()
        {
            _onConfirm?.Invoke();
            CloseSelf();
        }

        private void Cancel()
        {
            _onCancel?.Invoke();
            CloseSelf();
        }

        private void CloseSelf()
        {
            // Confirm dialog is always pushed onto UI stack, so pop it to close.
            // (UIService/Stack owns the lifetime of the window instance.)
            ZXTemplate.Core.ServiceContainer.Get<IUIService>().Pop();
        }
    }
}
