using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.UI
{
    public class UIOverlay : MonoBehaviour
    {
        private readonly Dictionary<string, UIWindow> _channels = new();

        public UIWindow Show(string channel, UIWindow prefab)
        {
            if (string.IsNullOrEmpty(channel)) channel = "Default";
            Clear(channel);

            var inst = Instantiate(prefab, transform);
            inst.OnPushed();
            _channels[channel] = inst;
            return inst;
        }

        public void Clear(string channel = "Default")
        {
            if (string.IsNullOrEmpty(channel)) channel = "Default";
            if (!_channels.TryGetValue(channel, out var win) || win == null) return;

            win.OnPopped();
            Destroy(win.gameObject);
            _channels.Remove(channel);
        }

        public void ClearAll()
        {
            foreach (var kv in _channels)
            {
                if (kv.Value == null) continue;
                kv.Value.OnPopped();
                Destroy(kv.Value.gameObject);
            }
            _channels.Clear();
        }
    }
}
