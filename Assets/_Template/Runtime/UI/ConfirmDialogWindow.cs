using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;

namespace ZXTemplate.UI
{
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

            confirmButton.onClick.AddListener(Confirm);
            cancelButton.onClick.AddListener(Cancel);
        }

        public override void OnPopped()
        {
            _running = false;

            confirmButton.onClick.RemoveListener(Confirm);
            cancelButton.onClick.RemoveListener(Cancel);
        }

        private void Update()
        {
            if (!_running) return;
            if (_timeout <= 0f) return;

            _timeout -= Time.unscaledDeltaTime;
            UpdateTimerLabel(_timeout);

            if (_timeout <= 0f)
            {
                // timeout -> treat as cancel by default
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
            // use UI stack
            ServiceContainer.Get<IUIService>().Pop();
        }
    }
}
