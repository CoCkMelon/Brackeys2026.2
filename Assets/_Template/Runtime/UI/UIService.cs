namespace ZXTemplate.UI
{
    /// <summary>
    /// High-level UI API used by game code.
    ///
    /// This service wraps:
    /// - UIStack: modal windows/pages (Settings, Pause, MainMenu, etc.)
    /// - UIOverlay: non-stacked UI (HUD, Toast, always-on widgets)
    /// - Loading: a simple loading indicator panel
    ///
    /// Rule of thumb:
    /// - Use Push/Pop for "screens" that block interaction behind them.
    /// - Use ShowOverlay for UI that should stay visible or be shown independently.
    /// </summary>
    public class UIService : IUIService
    {
        public UIRoot Root { get; }

        public UIService(UIRoot root) => Root = root;

        // -----------------------
        // Stack windows (LIFO)
        // -----------------------
        public UIWindow Push(UIWindow windowPrefab) => Root.Stack.Push(windowPrefab);
        public void Pop() => Root.Stack.Pop();
        public void Clear() => Root.Stack.Clear();

        // -----------------------
        // Overlay windows (by channel)
        // -----------------------
        public UIWindow ShowOverlay(UIWindow prefab) => Root.Overlay.Show("Default", prefab);
        public UIWindow ShowOverlay(string channel, UIWindow prefab) => Root.Overlay.Show(channel, prefab);

        public void ClearOverlay() => Root.Overlay.Clear("Default");
        public void ClearOverlay(string channel) => Root.Overlay.Clear(channel);
        public void ClearAllOverlays() => Root.Overlay.ClearAll();

        // -----------------------
        // Loading indicator
        // -----------------------
        public void ShowLoading(bool show)
        {
            if (Root.Loading == null) return;
            if (show) Root.Loading.Show();
            else Root.Loading.Hide();
        }
    }
}
