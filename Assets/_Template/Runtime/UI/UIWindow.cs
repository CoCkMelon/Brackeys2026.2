using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// Base class for all UI windows in this template.
    ///
    /// Lifecycle:
    /// - OnPushed(): Called when the window is instantiated and becomes active (top of stack or overlay shown).
    /// - OnPopped(): Called right before the window is destroyed/removed.
    ///
    /// Recommended usage:
    /// - Bind UI events in OnPushed() (button.onClick += ...)
    /// - Unbind UI events in OnPopped() (button.onClick -= ...)
    /// - Do NOT rely on OnEnable/OnDisable for window lifecycle, because:
    ///   - UIStack hides the previous window via SetActive(false)
    ///   - Overlay may replace windows without disabling the whole UI root
    /// </summary>
    public class UIWindow : MonoBehaviour
    {
        /// <summary>
        /// Called when this window is pushed/shown.
        /// Use it to bind UI events and initialize state.
        /// </summary>
        public virtual void OnPushed() { }

        /// <summary>
        /// Called when this window is popped/hidden permanently (before destroy).
        /// Use it to unbind UI events and cleanup.
        /// </summary>
        public virtual void OnPopped() { }
    }
}
