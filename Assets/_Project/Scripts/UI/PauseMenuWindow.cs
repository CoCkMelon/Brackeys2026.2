using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Scenes;
using ZXTemplate.UI;

/// <summary>
/// Pause menu window (modal, on UIStack).
///
/// Behavior on open:
/// - Acquire a pause token (Time.timeScale -> 0)
/// - Acquire an input mode token (force UI action map)
///
/// Behavior on close:
/// - Release both tokens to restore previous state.
///   (If other systems are also pausing / forcing UI mode, their tokens keep it paused/UI.)
///
/// Buttons:
/// - Resume: closes this window (Pop)
/// - Settings: opens SettingsWindow on top of pause menu
/// </summary>
public class PauseMenuWindow : UIWindow
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private SettingsWindow settingsWindowPrefab;

    private ISceneService _scene;
    private IPauseService _pause;
    private IInputModeService _inputMode;

    private object _pauseToken;
    private object _inputToken;

    public override void OnPushed()
    {
        _pause = ServiceContainer.Get<IPauseService>();
        _inputMode = ServiceContainer.Get<IInputModeService>();
        _scene = ServiceContainer.Get<ISceneService>();

        // Acquire pause token (token-based pause prevents "pause ownership" bugs).
        _pauseToken = _pause.Acquire("PauseMenu");

        // Force UI input while the menu is open.
        _inputToken = _inputMode.Acquire(InputMode.UI, "PauseMenu");

        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
    }

    public override void OnPopped()
    {
        resumeButton.onClick.RemoveListener(Resume);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitButton.onClick.RemoveListener(Quit);

        // Release tokens (safe even if other systems still hold tokens).
        if (_pauseToken != null)
        {
            _pause.Release(_pauseToken);
            _pauseToken = null;
        }

        if (_inputToken != null)
        {
            _inputMode.Release(_inputToken);
            _inputToken = null;
        }
    }

    private void Resume()
    {
        // Close pause menu (pop from UI stack).
        ServiceContainer.Get<IUIService>().Pop();
    }

    private void OpenSettings()
    {
        // SettingsWindow will acquire its own UI/pause tokens based on its configuration.
        ServiceContainer.Get<IUIService>().Push(settingsWindowPrefab);
    }

    private async void Quit()
    {
        var scene = ServiceContainer.Get<ISceneService>();
        await scene.LoadSceneAsync("MainMenu");
    }
}
