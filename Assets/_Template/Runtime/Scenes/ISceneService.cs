using System.Threading.Tasks;

namespace ZXTemplate.Scenes
{
    public interface ISceneService
    {
        Task LoadSceneAsync(string sceneName);
    }
}
