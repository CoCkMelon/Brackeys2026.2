using UnityEngine;

namespace ZXTemplate.UI
{
    public class UIOverlay : MonoBehaviour
    {
        private UIWindow _current;

        public UIWindow Show(UIWindow prefab)
        {
            Clear();
            _current = Instantiate(prefab, transform);
            _current.OnPushed();
            return _current;
        }

        public void Clear()
        {
            if (_current == null) return;

            _current.OnPopped();
            Destroy(_current.gameObject);
            _current = null;
        }
    }
}
