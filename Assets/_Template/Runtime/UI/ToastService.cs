using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.UI
{
    public class ToastService : IToastService
    {
        private readonly IUIService _ui;
        private readonly ToastView _prefab;

        private readonly Queue<Item> _queue = new();
        private ToastView _current;
        private float _timer;
        private float _fadeIn = 0.12f;
        private float _fadeOut = 0.18f;

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

            // 用一个 Runner 来驱动 Update（不依赖场景）
            var go = new GameObject("@ToastService");
            Object.DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.DontSave;
            go.AddComponent<ToastServiceRunner>().Init(this);
        }

        public void Show(string message, float seconds = 2f)
        {
            if (string.IsNullOrEmpty(message)) return;
            seconds = Mathf.Max(0.5f, seconds);

            _queue.Enqueue(new Item { msg = message, seconds = seconds });

            if (!_showing)
                Next();
        }

        public void Clear()
        {
            _queue.Clear();
            HideImmediate();
        }

        internal void Tick(float dtUnscaled)
        {
            if (_current == null) return;

            _timer += dtUnscaled;

            // timeline: fadein -> hold -> fadeout
            float fadeInEnd = _fadeIn;
            float holdEnd = _fadeIn + _currentHold;
            float fadeOutEnd = _fadeIn + _currentHold + _fadeOut;

            float a;
            if (_timer <= fadeInEnd)
                a = Mathf.Clamp01(_timer / _fadeIn);
            else if (_timer <= holdEnd)
                a = 1f;
            else if (_timer <= fadeOutEnd)
                a = Mathf.Clamp01(1f - ((_timer - holdEnd) / _fadeOut));
            else
            {
                // done
                HideImmediate();
                Next();
                return;
            }

            _current.SetAlpha(a);
        }

        private float _currentHold;

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

            // 确保 overlay 有 view
            _current = (ToastView)_ui.ShowOverlay("Toast", _prefab);
            _current.SetMessage(item.msg);
            _current.SetAlpha(0f);
            _timer = 0f;
        }

        private void HideImmediate()
        {
            if (_current == null) return;

            _ui.ClearOverlay("Toast");
            _current = null;
            _timer = 0f;
        }

        // 内部 Runner
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
