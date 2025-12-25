namespace ZXTemplate.UI
{
    public interface IToastService
    {
        void Show(string message, float seconds = 2f);
        void Clear();
    }
}
