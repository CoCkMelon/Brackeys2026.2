using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZXTemplate.UI;

namespace ZXTemplate.Scenes
{
    /// <summary>
    /// Handles scene transitions for the template.
    ///
    /// Responsibilities:
    /// - Clear UI stack and overlays before loading (avoid UI persisting across scenes by accident).
    /// - Show a loading indicator during the async load operation.
    ///
    /// Notes:
    /// - This uses Task.Yield() to keep the UI responsive while loading.
    /// - If you later add fade-in/out or loading progress, this is the best place to do it.
    /// </summary>
    public class SceneService : ISceneService
    {
        private readonly IUIService _ui;

        public SceneService(IUIService ui) => _ui = ui;

        /// <summary>
        /// Loads a scene asynchronously by name.
        /// UI behavior during load:
        /// 1) Clear overlays (HUD/Toast) so we don't carry over the previous scene's UI.
        /// 2) Clear window stack (Settings/Pause/etc.) to reset navigation state.
        /// 3) Show loading screen until SceneManager finishes.
        /// </summary>
        public async Task LoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) throw new System.ArgumentException("sceneName is null or empty.");

            // Reset UI state before load (prevents old windows from sticking around).
            _ui.ClearAllOverlays();
            _ui.Clear();

            // Show loading UI immediately.
            _ui.ShowLoading(true);

            // Start loading.
            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = true;

            // Await completion without blocking the main thread.
            while (!op.isDone)
                await Task.Yield();

            // Hide loading UI after scene is active.
            _ui.ShowLoading(false);

            Debug.Log($"[SceneService] Active Scene = {SceneManager.GetActiveScene().name}");
        }
    }
}
