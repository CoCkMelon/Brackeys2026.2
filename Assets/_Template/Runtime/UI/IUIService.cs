namespace ZXTemplate.UI
{
    public interface IUIService
    {
        UIRoot Root { get; } 
        UIWindow Push(UIWindow windowPrefab); 
        void Pop(); 
        void Clear(); 

        UIWindow ShowOverlay(UIWindow prefab); 
        UIWindow ShowOverlay(string channel, UIWindow prefab);
        void ClearOverlay(); 
        void ClearOverlay(string channel);
        void ClearAllOverlays();
        void ShowLoading(bool show); 
    }
}
