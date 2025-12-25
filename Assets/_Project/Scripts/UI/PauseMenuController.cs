using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.UI;

/// <summary>
/// Listens to the Pause input action and toggles the PauseMenuWindow on the UI stack.
///
/// Responsibilities:
/// - Subscribe to IInputService.OnPause
/// - Push the pause menu prefab when pause is pressed
/// - Pop the top window when pause is pressed again (close menu)
///
/// Notes / assumptions:
/// - This controller assumes the pause menu is the top window when it is open.
/// - It tracks an instance reference (_openedWindow) as the "is open" flag.
///   If other code clears the UI stack or pops the window externally,
///   _openedWindow could become out of sync. For a template project this is OK,
///   but in larger projects you may want to query UIStack state instead.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private PauseMenuWindow pauseMenuPrefab;

    private IUIService _ui;
    private IInputService _input;

    // Used as a lightweight "is open" flag.
    private UIWindow _openedWindow;

    private void Start()
    {
        _ui = ServiceContainer.Get<IUIService>();
        _input = ServiceContainer.Get<IInputService>();

        _input.OnPause += TogglePauseMenu;
    }

    private void OnDestroy()
    {
        if (_input != null)
            _input.OnPause -= TogglePauseMenu;
    }

    private void TogglePauseMenu()
    {
        // Already open -> close.
        if (_openedWindow != null)
        {
            _ui.Pop();
            _openedWindow = null;
            return;
        }

        // Not open -> open.
        _openedWindow = _ui.Push(pauseMenuPrefab);
    }
}
