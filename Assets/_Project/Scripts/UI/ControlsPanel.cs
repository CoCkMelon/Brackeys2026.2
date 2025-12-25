using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;
using ZXTemplate.UI;

/// <summary>
/// Controls settings panel that manages multiple RebindActionRow entries.
///
/// Responsibilities:
/// - Refresh all rows when the panel becomes active (so binding display is correct).
/// - Provide a "Reset All" button with confirm dialog + toast feedback.
///
/// Data flow on reset:
/// 1) Clear runtime binding overrides on InputActionAsset
/// 2) Clear saved overrides JSON in SettingsData.controls
/// 3) Apply + Save settings so it persists across sessions
/// 4) Refresh UI rows
/// </summary>
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
        // Resolve services created in Bootstrapper.
        _input = ServiceContainer.Get<IInputService>();
        _settings = ServiceContainer.Get<ISettingsService>();
        _confirm = ServiceContainer.Get<IConfirmService>();
        _toast = ServiceContainer.Get<IToastService>();

        if (resetAllButton) resetAllButton.onClick.AddListener(OnClickResetAll);

        // Refresh in next frame to ensure child rows have finished OnEnable()
        // and layout has stabilized (avoids stale binding UI).
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

    /// <summary>
    /// Refreshes all rows' UI text (action name + binding display string).
    /// </summary>
    public void RefreshAll()
    {
        if (rows == null) return;

        for (int i = 0; i < rows.Length; i++)
            if (rows[i]) rows[i].RefreshUI();
    }

    /// <summary>
    /// Reset all bindings -> show confirm dialog first.
    /// </summary>
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

    /// <summary>
    /// Performs the reset:
    /// - Clears runtime overrides (immediately affects current session)
    /// - Clears saved override json (affects next sessions)
    /// - Applies + saves settings
    /// - Refreshes UI and shows a toast
    /// </summary>
    private void ResetAllNow()
    {
        // 1) Clear overrides in runtime asset.
        _input.Actions.RemoveAllBindingOverrides();

        // 2) Clear saved overrides.
        _settings.Data.controls.bindingOverridesJson = "";
        _settings.MarkDirty();

        // Apply ensures InputActionAsset is in default state even if other appliers exist.
        _settings.ApplyAll();

        // Persist to disk.
        _settings.Save();

        // 3) Refresh UI.
        RefreshAll();

        // 4) Feedback.
        _toast.Show("Key bindings reset", 1.5f);
    }
}
