using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.UI
{
    /// <summary>
    /// A simple LIFO stack for modal UI windows.
    ///
    /// Behavior:
    /// - Push(): Instantiates a new window and hides the previous top window.
    /// - Pop(): Destroys the top window and re-shows the previous window.
    /// - Clear(): Destroys all windows.
    ///
    /// Why hide instead of destroy on Push:
    /// - Some windows may want to preserve state while a modal window is open.
    /// - Faster than re-instantiating when returning (for small stacks).
    ///
    /// Note:
    /// - Windows must implement UIWindow.OnPushed/OnPopped for binding/unbinding UI events.
    /// </summary>
    public class UIStack : MonoBehaviour
    {
        private readonly Stack<UIWindow> _stack = new();

        /// <summary>
        /// Push a window prefab onto the stack and make it the active top window.
        /// </summary>
        public UIWindow Push(UIWindow windowPrefab)
        {
            var instance = Instantiate(windowPrefab, transform);

            // Hide current top window so only one "screen" is interactive.
            if (_stack.TryPeek(out var top))
                top.gameObject.SetActive(false);

            _stack.Push(instance);
            instance.OnPushed();
            return instance;
        }

        /// <summary>
        /// Pop the current top window. The previous one becomes active again.
        /// </summary>
        public void Pop()
        {
            if (_stack.Count == 0) return;

            var top = _stack.Pop();
            top.OnPopped();
            Destroy(top.gameObject);

            if (_stack.TryPeek(out var next))
                next.gameObject.SetActive(true);
        }

        /// <summary>
        /// Clear all windows from the stack (destroy instances).
        /// </summary>
        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var w = _stack.Pop();
                w.OnPopped();
                Destroy(w.gameObject);
            }
        }
    }
}
