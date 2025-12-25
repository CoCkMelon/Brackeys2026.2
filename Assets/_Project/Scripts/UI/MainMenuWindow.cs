using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Scenes;
using ZXTemplate.UI;

/// <summary>
/// Main Menu window (UIStack).
///
/// Buttons:
/// - Start: loads the gameplay scene via ISceneService (shows loading screen, clears UI, etc.)
/// - Settings: pushes SettingsWindow (no pause required in main menu)
/// - Quit: exits application (or stops play mode in editor)
///
/// Lifecycle:
/// - OnPushed: bind button events
/// - OnPopped: unbind button events
/// </summary>
public class MainMenuWindow : UIWindow
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Prefabs")]
    [SerializeField] private SettingsWindow settingsWindowPrefab;

    public override void OnPushed()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OpenSettings);
        quitButton.onClick.AddListener(Quit);
    }

    public override void OnPopped()
    {
        startButton.onClick.RemoveListener(OnStartClicked);
        settingsButton.onClick.RemoveListener(OpenSettings);
        quitButton.onClick.RemoveListener(Quit);
    }

    /// <summary>
    /// Loads gameplay scene asynchronously.
    /// SceneService is responsible for clearing UI and showing loading screen.
    /// </summary>
    private async void OnStartClicked()
    {
        var scenes = ServiceContainer.Get<ISceneService>();
        await scenes.LoadSceneAsync(gameSceneName);
    }

    /// <summary>
    /// Opens the Settings window.
    ///
    /// Note:
    /// - In main menu we usually do NOT pause the game (no gameplay running),
    ///   so using a SettingsWindow prefab with pauseGameOnOpen=false is simplest.
    /// </summary>
    private void OpenSettings()
    {
        ServiceContainer.Get<IUIService>().Push(settingsWindowPrefab);
    }

    /// <summary>
    /// Quits the application (standalone build) or stops Play Mode (Unity Editor).
    /// </summary>
    private void Quit()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
