using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Input;

/// <summary>
/// Scene-level installer for the Game scene.
///
/// Responsibility:
/// - Set the base input mode to Gameplay when entering the game scene.
///
/// Why BaseMode:
/// - InputModeService uses token overrides (UI windows like Pause/Settings can temporarily
///   force UI mode). BaseMode defines what mode the game returns to after all UI tokens are released.
///
/// Notes:
/// - We intentionally do NOT call IInputService.EnableGameplay() directly here,
///   because InputModeService should be the single source of truth for which action maps are enabled.
/// </summary>
public class GameInstaller : MonoBehaviour
{
    private void Start()
    {
        ServiceContainer.Get<IInputModeService>().SetBaseMode(InputMode.Gameplay);
    }
}
