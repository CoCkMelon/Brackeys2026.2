using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Scenes;

/// <summary>
/// Entry point for the Bootstrap scene.
///
/// How to use:
/// - Create a lightweight "Bootstrap" scene that contains:
///     - Bootstrapper (register services + create persistent roots)
///     - BootstrapEntry (loads the first real scene)
///
/// Why:
/// - Ensures services are initialized once (DontDestroyOnLoad) before any other scene logic runs.
/// - Avoids duplicated managers when loading MainMenu/Game directly in editor.
///
/// Behavior:
/// - On Start, it asks ISceneService to load the configured first scene.
/// - SceneService typically shows a loading screen, clears UI stack, and performs async load.
/// </summary>
public class BootstrapEntry : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainMenu";

    private async void Start()
    {
        var scenes = ServiceContainer.Get<ISceneService>();
        await scenes.LoadSceneAsync(firstSceneName);
    }
}
