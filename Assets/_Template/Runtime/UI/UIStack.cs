using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.UI
{
    public class UIStack : MonoBehaviour
    {
        private readonly Stack<UIWindow> _stack = new();

        public UIWindow Push(UIWindow windowPrefab)
        {
            var instance = Instantiate(windowPrefab, transform);
            if (_stack.TryPeek(out var top))
                top.gameObject.SetActive(false);

            _stack.Push(instance);
            instance.OnPushed();
            return instance;
        }

        public void Pop()
        {
            if (_stack.Count == 0) return;

            var top = _stack.Pop();
            top.OnPopped();
            Destroy(top.gameObject);

            if (_stack.TryPeek(out var next))
                next.gameObject.SetActive(true);
        }

        public void Clear()
        {
            while (_stack.Count > 0)
            {
                var w = _stack.Pop();
                Destroy(w.gameObject);
            }
        }
    }
}
