namespace ZXTemplate.UI
{
    /// <summary>
    /// High-level UI API used by game code.
    ///
    /// Provides:
    /// - UIStack (Push/Pop/Clear) for modal windows (Pause, Settings, Menus)
    /// - UIOverlay (Show/Clear by channel) for non-stacked UI (HUD, Toast)
    /// - LoadingScreen toggling
    ///
    /// Conventions:
    /// - Push() instantiates a window prefab under the UIStack root.
    /// - Overlays are shown by channel, so different systems can coexist (e.g. "HUD" vs "Toast").
    /// </summary>
    public interface IUIService
    {
        UIRoot Root { get; }

        // Stack windows (LIFO)
        UIWindow Push(UIWindow windowPrefab);
        void Pop();
        void Clear();

        // Overlays (by channel)
        UIWindow ShowOverlay(UIWindow prefab);
        UIWindow ShowOverlay(string channel, UIWindow prefab);
        void ClearOverlay();
        void ClearOverlay(string channel);
        void ClearAllOverlays();

        // Loading indicator
        void ShowLoading(bool show);
    }
}
