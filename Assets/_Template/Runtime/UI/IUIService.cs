
namespace ZXTemplate.UI
{
    public interface IUIService
    {
        UIRoot Root { get; }
        UIWindow Push(UIWindow windowPrefab);
        void Pop();
        void Clear();
        void ShowLoading(bool show);
    }
}
