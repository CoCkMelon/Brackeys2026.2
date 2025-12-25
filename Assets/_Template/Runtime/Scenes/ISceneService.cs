using System.Threading.Tasks;

namespace ZXTemplate.Scenes
{
    /// <summary>
    /// Scene transition API for the template.
    ///
    /// Notes:
    /// - Implementations may clear UI stack/overlays and show a loading screen.
    /// - Async is used to keep UI responsive during loading.
    /// </summary>
    public interface ISceneService
    {
        /// <summary>Loads a scene asynchronously by name.</summary>
        Task LoadSceneAsync(string sceneName);
    }
}
