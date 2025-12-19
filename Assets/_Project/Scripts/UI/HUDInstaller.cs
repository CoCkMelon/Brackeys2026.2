using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.UI;

public class HUDInstaller : MonoBehaviour
{
    [SerializeField] private UIWindow hudPrefab;

    private void Start()
    {
        ServiceContainer.Get<IUIService>().ShowOverlay(hudPrefab);
    }

    private void OnDestroy()
    {
        // optional safety
        if (ServiceContainer.TryGet<IUIService>(out var ui))
            ui.ClearOverlay();
    }
}
