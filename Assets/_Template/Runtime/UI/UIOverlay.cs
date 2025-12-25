using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// Overlay layer for UI that should NOT be part of the window stack.
    ///
    /// Use cases:
    /// - HUD (always visible during gameplay)
    /// - Toast notifications
    /// - Small persistent widgets that should not be "pushed/popped" like windows
    ///
    /// Design:
    /// - Multiple channels are supported (e.g. "HUD", "Toast").
    /// - Each channel can hold at most one UIWindow instance.
    /// - Showing a new overlay on the same channel automatically replaces the previous one.
    /// </summary>
    public class UIOverlay : MonoBehaviour
    {
        // channel -> overlay instance
        private readonly Dictionary<string, UIWindow> _channels = new();

        /// <summary>
        /// Shows an overlay window on a given channel.
        /// If the channel already has an overlay, it will be replaced.
        /// </summary>
        public UIWindow Show(string channel, UIWindow prefab)
        {
            channel = NormalizeChannel(channel);
            Clear(channel);

            var inst = Instantiate(prefab, transform);
            inst.OnPushed();
            _channels[channel] = inst;
            return inst;
        }

        /// <summary>
        /// Clears (destroys) the overlay on the given channel if it exists.
        /// </summary>
        public void Clear(string channel = "Default")
        {
            channel = NormalizeChannel(channel);

            if (!_channels.TryGetValue(channel, out var win) || win == null)
            {
                // Keep dictionary clean if Unity destroyed the object externally.
                _channels.Remove(channel);
                return;
            }

            win.OnPopped();
            Destroy(win.gameObject);
            _channels.Remove(channel);
        }

        /// <summary>
        /// Clears all overlays on all channels.
        /// </summary>
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

        private static string NormalizeChannel(string channel)
            => string.IsNullOrEmpty(channel) ? "Default" : channel;
    }
}
