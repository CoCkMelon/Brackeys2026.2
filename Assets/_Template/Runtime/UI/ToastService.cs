using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// Shows small, non-blocking toast messages.
    ///
    /// Design goals:
    /// - Non-intrusive: does not block raycasts (handled by ToastView CanvasGroup on prefab).
    /// - Works during pause: uses unscaledDeltaTime for timing.
    /// - Scene-independent: driven by an internal runner GameObject marked DontDestroyOnLoad.
    /// - Queue-based: multiple Show() calls are queued and displayed sequentially.
    ///
    /// Implementation notes:
    /// - Toast is displayed via UIOverlay channel "Toast" so it will not interfere with HUD overlay.
    /// - Each toast has: fade-in -> hold -> fade-out.
    /// </summary>
    public class ToastService : IToastService
    {
        private readonly IUIService _ui;
        private readonly ToastView _prefab;

        // FIFO queue so multiple toast requests don't overwrite each other.
        private readonly Queue<Item> _queue = new();

        private ToastView _current;
        private float _timer;

        // Animation timings (seconds).
        private float _fadeIn = 0.12f;
        private float _fadeOut = 0.18f;

        private float _currentHold;
        private bool _showing;

        private struct Item
        {
            public string msg;
            public float seconds;
        }

        public ToastService(IUIService ui, ToastView prefab)
        {
            _ui = ui;
            _prefab = prefab;

            // A tiny runner to drive Update without needing a scene object.
            // This keeps toast working across scene loads.
            var go = new GameObject("@ToastService");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.DontSave;

            go.AddComponent<ToastServiceRunner>().Init(this);
        }

        /// <summary>
        /// Enqueue a toast message.
        /// seconds = hold duration (not counting fade-in/out). Minimum is clamped.
        /// </summary>
        public void Show(string message, float seconds = 2f)
        {
            if (string.IsNullOrEmpty(message)) return;

            seconds = Mathf.Max(0.5f, seconds);
            _queue.Enqueue(new Item { msg = message, seconds = seconds });

            // If nothing is currently showing, start immediately.
            if (!_showing)
                Next();
        }

        /// <summary>
        /// Clears the queue and hides the current toast immediately.
        /// </summary>
        public void Clear()
        {
            _queue.Clear();
            HideImmediate();
        }

        /// <summary>
        /// Tick called by internal runner. dtUnscaled ensures toast works during pause.
        /// </summary>
        internal void Tick(float dtUnscaled)
        {
            if (_current == null) return;

            _timer += dtUnscaled;

            // timeline: fade-in -> hold -> fade-out
            float fadeInEnd = _fadeIn;
            float holdEnd = _fadeIn + _currentHold;
            float fadeOutEnd = _fadeIn + _currentHold + _fadeOut;

            float a;

            if (_timer <= fadeInEnd)
            {
                a = Mathf.Clamp01(_timer / _fadeIn);
            }
            else if (_timer <= holdEnd)
            {
                a = 1f;
            }
            else if (_timer <= fadeOutEnd)
            {
                a = Mathf.Clamp01(1f - ((_timer - holdEnd) / _fadeOut));
            }
            else
            {
                // Current toast finished.
                HideImmediate();
                Next();
                return;
            }

            _current.SetAlpha(a);
        }

        /// <summary>
        /// Shows the next toast from queue.
        /// </summary>
        private void Next()
        {
            if (_queue.Count == 0)
            {
                _showing = false;
                return;
            }

            _showing = true;

            var item = _queue.Dequeue();
            _currentHold = item.seconds;

            // Show on a dedicated overlay channel.
            _current = (ToastView)_ui.ShowOverlay("Toast", _prefab);
            _current.SetMessage(item.msg);
            _current.SetAlpha(0f);

            _timer = 0f;
        }

        /// <summary>
        /// Hides the current toast immediately (no animation).
        /// </summary>
        private void HideImmediate()
        {
            if (_current == null) return;

            _ui.ClearOverlay("Toast");
            _current = null;
            _timer = 0f;
        }

        /// <summary>
        /// Internal runner that drives Tick() from Unity Update.
        /// This avoids requiring any scene object and survives scene loads.
        /// </summary>
        private class ToastServiceRunner : MonoBehaviour
        {
            private ToastService _svc;

            public void Init(ToastService svc) => _svc = svc;

            private void Update()
            {
                if (_svc == null) return;
                _svc.Tick(Time.unscaledDeltaTime);
            }
        }
    }
}
