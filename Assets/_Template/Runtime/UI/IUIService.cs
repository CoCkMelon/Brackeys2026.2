namespace ZXTemplate.UI
{
    public interface IUIService
    {
        UIRoot Root { get; } 
        UIWindow Push(UIWindow windowPrefab); 
        void Pop(); 
        void Clear(); 

        UIWindow ShowOverlay(UIWindow prefab); 
        void ClearOverlay(); 

        void ShowLoading(bool show); 
    }
}
