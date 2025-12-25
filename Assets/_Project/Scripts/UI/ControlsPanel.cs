using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;
using ZXTemplate.UI;

public class ControlsPanel : MonoBehaviour
{
    [SerializeField] private Button resetAllButton;
    [SerializeField] private RebindActionRow[] rows;

    private IInputService _input;
    private ISettingsService _settings;
    private IConfirmService _confirm;
    private IToastService _toast;

    private void OnEnable()
    {
        _input = ServiceContainer.Get<IInputService>();
        _settings = ServiceContainer.Get<ISettingsService>();
        _confirm = ServiceContainer.Get<IConfirmService>();
        _toast = ServiceContainer.Get<IToastService>();

        if (resetAllButton) resetAllButton.onClick.AddListener(OnClickResetAll);

        StartCoroutine(RefreshNextFrame());
    }

    private System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null;
        RefreshAll();
    }


    private void OnDisable()
    {
        if (resetAllButton) resetAllButton.onClick.RemoveListener(OnClickResetAll);
    }

    public void RefreshAll()
    {
        if (rows == null) return;
        for (int i = 0; i < rows.Length; i++)
            if (rows[i]) rows[i].RefreshUI();
    }

    private void OnClickResetAll()
    {
        _confirm.Show(
            title: "Reset Controls",
            message: "Reset ALL key bindings to default?",
            confirmText: "Reset",
            cancelText: "Cancel",
            onConfirm: ResetAllNow,
            onCancel: null
        );
    }

    private void ResetAllNow()
    {
        // 1) clear overrides in runtime
        _input.Actions.RemoveAllBindingOverrides();

        // 2) clear saved overrides
        _settings.Data.controls.bindingOverridesJson = "";
        _settings.MarkDirty();
        _settings.ApplyAll();
        _settings.Save();

        // 3) refresh UI
        RefreshAll();

        // 4) toast
        _toast.Show("Key bindings reset", 1.5f);
    }
}
