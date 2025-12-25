using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private Button resetAllButton;
    [SerializeField] private RebindActionRow[] rows;

    private IInputService _input;
    private ISettingsService _settings;

    private void OnEnable()
    {
        _input = ServiceContainer.Get<IInputService>();
        _settings = ServiceContainer.Get<ISettingsService>();

        if (resetAllButton) resetAllButton.onClick.AddListener(ResetAll);

        RefreshAll();
    }

    private void OnDisable()
    {
        if (resetAllButton) resetAllButton.onClick.RemoveListener(ResetAll);
    }

    public void RefreshAll()
    {
        if (rows == null) return;
        for (int i = 0; i < rows.Length; i++)
            if (rows[i]) rows[i].RefreshUI();
    }

    private void ResetAll()
    {
        _input.Actions.RemoveAllBindingOverrides();

        _settings.Data.controls.bindingOverridesJson = "";
        _settings.MarkDirty();
        _settings.Save();

        RefreshAll();
    }
}
