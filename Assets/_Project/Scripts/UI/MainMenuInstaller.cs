using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.UI;

/// <summary>
/// Scene-level installer for the Main Menu scene.
///
/// Responsibilities:
/// - Set base input mode to UI (so menu navigation works by default).
/// - Push the MainMenuWindow onto the UIStack.
///
/// Notes:
/// - We use InputModeService.SetBaseMode instead of directly enabling maps,
///   because token-based InputModeService will manage UI/Game switching consistently.
/// </summary>
public class MainMenuInstaller : MonoBehaviour
{
    [SerializeField] private MainMenuWindow mainMenuPrefab;

    private void Start()
    {
        ServiceContainer.Get<IInputModeService>().SetBaseMode(InputMode.UI);
        ServiceContainer.Get<IUIService>().Push(mainMenuPrefab);
    }
}
