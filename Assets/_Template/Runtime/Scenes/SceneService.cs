using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using ZXTemplate.UI;

namespace ZXTemplate.Scenes
{
    public class SceneService : ISceneService
    {
        private readonly IUIService _ui;

        public SceneService(IUIService ui) => _ui = ui;

        public async Task LoadSceneAsync(string sceneName)
        {
            _ui.ShowLoading(true);

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = true;

            while (!op.isDone)
                await Task.Yield();

            _ui.ShowLoading(false);
        }
    }
}
