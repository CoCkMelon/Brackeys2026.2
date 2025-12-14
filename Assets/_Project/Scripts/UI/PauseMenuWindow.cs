using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.UI;

public class PauseMenuWindow : UIWindow
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private SettingsWindow settingsWindowPrefab;

    private IPauseService _pause;
    private IInputService _input;

    private object _pauseToken;

    public override void OnPushed()
    {
        _pause = ServiceContainer.Get<IPauseService>();
        _input = ServiceContainer.Get<IInputService>();

        // 获得一个暂停 token（引用计数）
        _pauseToken = _pause.Acquire("PauseMenu");
        _input.EnableUI();

        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);

    }

    public override void OnPopped()
    {
        resumeButton.onClick.RemoveListener(Resume);
        settingsButton.onClick.RemoveListener(OpenSettings);

        // 释放 token
        _pause.Release(_pauseToken);
        _pauseToken = null;

        _input.EnableGameplay();

    }

    private void Resume()
    {
        // 由控制器负责 Pop（更统一），这里也可以直接 Pop
        ServiceContainer.Get<IUIService>().Pop();
    }

    private void OpenSettings()
    {
        ServiceContainer.Get<IUIService>().Push(settingsWindowPrefab);
    }
}
