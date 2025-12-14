using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Scenes;
using ZXTemplate.UI;

public class MainMenuWindow : UIWindow
{
    [SerializeField] private Button startButton;
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsWindow settingsWindowPrefab;

    public override void OnPushed()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OpenSettings);
    }

    public override void OnPopped()
    {
        startButton.onClick.RemoveListener(OnStartClicked);
        settingsButton.onClick.RemoveListener(OpenSettings);
    }

    private async void OnStartClicked()
    {
        var scenes = ServiceContainer.Get<ISceneService>();
        await scenes.LoadSceneAsync(gameSceneName);
    }

    private void OpenSettings()
    {
        // MainMenu 里不需要暂停：做一个“MainMenu专用Prefab”最简单
        ServiceContainer.Get<IUIService>().Push(settingsWindowPrefab);
    }
}
