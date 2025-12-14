using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.UI;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private PauseMenuWindow pauseMenuPrefab;

    private IUIService _ui;
    private IInputService _input;

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
        // 已打开 -> 关闭
        if (_openedWindow != null)
        {
            _ui.Pop();
            _openedWindow = null;
            return;
        }

        // 未打开 -> 打开
        _openedWindow = _ui.Push(pauseMenuPrefab);
    }
}
