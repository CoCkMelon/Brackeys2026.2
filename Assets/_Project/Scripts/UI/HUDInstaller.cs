using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.UI;

/// <summary>
/// Scene-level installer for the HUD overlay.
///
/// Usage:
/// - Place this MonoBehaviour in your Game scene.
/// - Assign a HUDWindow prefab (as UIWindow) in inspector.
/// - On Start, it shows the HUD on UIOverlay channel "HUD".
///
/// Why overlay:
/// - HUD should not be part of the modal UI stack (Pause/Settings).
/// - HUD should remain visible and independent.
/// </summary>
public class HUDInstaller : MonoBehaviour
{
    [SerializeField] private UIWindow hudPrefab;

    private void Start()
    {
        ServiceContainer.Get<IUIService>().ShowOverlay("HUD", hudPrefab);
    }

    private void OnDestroy()
    {
        // Optional safety:
        // Clear the HUD overlay if this installer is destroyed (e.g. scene unload).
        if (ServiceContainer.TryGet<IUIService>(out var ui))
            ui.ClearOverlay("HUD"); // IMPORTANT: clear the same channel you used to show
    }
}
