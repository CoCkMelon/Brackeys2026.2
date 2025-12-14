using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Scenes;

public class BootstrapEntry : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";

    private async void Start()
    {
        var scenes = ServiceContainer.Get<ISceneService>();
        await scenes.LoadSceneAsync(firstSceneName);
    }
}
