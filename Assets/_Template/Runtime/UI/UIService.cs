namespace ZXTemplate.UI
{
    public class UIService : IUIService
    {
        public UIRoot Root { get; }

        public UIService(UIRoot root) => Root = root;

        public UIWindow Push(UIWindow windowPrefab) => Root.Stack.Push(windowPrefab);
        public void Pop() => Root.Stack.Pop();
        public void Clear() => Root.Stack.Clear();

        public UIWindow ShowOverlay(UIWindow prefab) => Root.Overlay.Show("Default", prefab);
        public UIWindow ShowOverlay(string channel, UIWindow prefab) => Root.Overlay.Show(channel, prefab);

        public void ClearOverlay() => Root.Overlay.Clear("Default");
        public void ClearOverlay(string channel) => Root.Overlay.Clear(channel);
        public void ClearAllOverlays() => Root.Overlay.ClearAll();

        public void ShowLoading(bool show)
        {
            if (Root.Loading == null) return;
            if (show) Root.Loading.Show();
            else Root.Loading.Hide();
        }
    }
}
