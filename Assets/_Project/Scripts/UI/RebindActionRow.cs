using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;
using ZXTemplate.UI;

/// <summary>
/// One row in the Controls settings page that rebinding ONE action binding.
///
/// What it does:
/// - Displays: Action name + current binding string
/// - Allows: interactive rebind for a specific binding index
/// - Allows: reset binding override for that index
///
/// Key concepts:
/// - actionPath: "ActionMap/ActionName" (example: "Gameplay/Jump")
/// - bindingIndex: which binding slot to rebind (usually 0 for keyboard)
///
/// Important (common pitfall you hit before):
/// - The dropdown in the prefab editor may reference an "actionsAsset" ONLY for editor convenience.
/// - At runtime, we MUST use IInputService.Actions (the asset assigned in Bootstrapper).
///   If the editor asset differs from runtime asset, actionPath might exist in editor but not in runtime.
/// </summary>
public class RebindActionRow : MonoBehaviour
{
    [Header("Binding")]
    [Tooltip("Example: Gameplay/Jump")]
    [SerializeField] private string actionPath = "Gameplay/Jump";

    [Tooltip("Which binding of the action to rebind (usually keyboard is 0)")]
    [SerializeField] private int bindingIndex = 0;

    /// <summary>
    /// Editor-only reference used by your dropdown tooling (not used by runtime logic).
    /// If this differs from runtime IInputService.Actions, you may see confusing mismatch warnings.
    /// </summary>
    [SerializeField] private InputActionAsset actionsAsset;

    [Header("UI")]
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text statusText; // optional

    private IInputService _input;
    private ISettingsService _settings;
    private IToastService _toast;

    private InputActionRebindingExtensions.RebindingOperation _op;
    private bool _wasActionEnabled;

    private void OnEnable()
    {
        // Resolve services. These are created in Bootstrapper and stored in ServiceContainer.
        _input = ServiceContainer.Get<IInputService>();
        _settings = ServiceContainer.Get<ISettingsService>();
        _toast = ServiceContainer.Get<IToastService>();

        if (rebindButton) rebindButton.onClick.AddListener(StartRebind);
        if (resetButton) resetButton.onClick.AddListener(ResetBinding);

        RefreshUI();
    }

    private void OnDisable()
    {
        if (rebindButton) rebindButton.onClick.RemoveListener(StartRebind);
        if (resetButton) resetButton.onClick.RemoveListener(ResetBinding);

        CancelIfRunning();
    }

    /// <summary>
    /// Finds the runtime action from the runtime InputActionAsset.
    /// Returns null if the asset or action does not exist.
    /// </summary>
    private InputAction GetAction()
    {
        if (_input == null)
            ServiceContainer.TryGet(out _input);

        var asset = _input?.Actions;
        if (asset == null) return null;

        // actionPath should be "Map/Action" (not just "Action").
        return asset.FindAction(actionPath, throwIfNotFound: false);
    }

    /// <summary>
    /// Updates UI texts for current action/binding state.
    /// Called when the row becomes active and after any rebind/reset.
    /// </summary>
    public void RefreshUI()
    {
        // Defensive: if runtime asset is not ready, show safe placeholder UI.
        if (_input?.Actions == null)
        {
            Debug.LogError(
                $"[RebindActionRow] Runtime IInputService.Actions is NULL on '{gameObject.name}'. " +
                $"Check Bootstrapper inputActions assignment and InputService constructor.");
            SafeSetTexts("(Input NULL)", "(N/A)");
            return;
        }

        // Warning for editor/runtime mismatch (very common source of "Action not found").
        if (actionsAsset != null && actionsAsset != _input.Actions)
        {
            Debug.LogWarning(
                $"[RebindActionRow] Mismatch InputActionAsset on '{gameObject.name}'. " +
                $"Editor actionsAsset='{actionsAsset.name}', Runtime actions='{_input.Actions.name}'. " +
                $"Dropdown may show actions not present at runtime.");
        }

        var action = GetAction();
        if (action == null)
        {
            Debug.LogError($"[RebindActionRow] Action not found: '{actionPath}' on '{gameObject.name}'.");
            SafeSetTexts("(Missing Action)", "(N/A)");
            return;
        }

        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError(
                $"[RebindActionRow] Invalid bindingIndex={bindingIndex} for '{actionPath}'. " +
                $"Bindings={action.bindings.Count} on '{gameObject.name}'.");
            SafeSetTexts(action.name, "(Invalid Index)");
            return;
        }

        if (actionNameText) actionNameText.text = action.name;
        if (bindingText) bindingText.text = action.GetBindingDisplayString(bindingIndex);
        if (statusText) statusText.text = "";
    }

    /// <summary>
    /// Starts interactive rebinding for the selected action/bindingIndex.
    /// Uses ESC to cancel.
    ///
    /// We temporarily:
    /// - disable the specific action
    /// - disable the whole asset to avoid UI navigation/click generating extra input
    /// - restore UI map afterwards (InputModeService keeps UI mode active)
    /// </summary>
    private void StartRebind()
    {
        CancelIfRunning();

        var action = GetAction();
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;

        // This row is designed for single non-composite bindings (e.g., Jump, Crouch).
        // Composite bindings (Move 2DVector) are intentionally not supported here.
        if (action.bindings[bindingIndex].isComposite || action.bindings[bindingIndex].isPartOfComposite)
        {
            _toast?.Show("Composite bindings are not supported in this row.", 2f);
            RefreshUI();
            return;
        }

        _wasActionEnabled = action.enabled;
        action.Disable();

        if (statusText) statusText.text = "Press a key... (ESC to cancel)";
        if (bindingText) bindingText.text = "...";

        // Disable the whole actions asset to prevent interference from UI navigation/click.
        _input.Actions.Disable();

        _op = action.PerformInteractiveRebinding(bindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(_ =>
            {
                CleanupAfterRebind(action);
                RefreshUI();
            })
            .OnComplete(_ =>
            {
                CleanupAfterRebind(action);

                // Persist overrides immediately into settings.
                SaveOverridesToSettings();

                RefreshUI();

                var display = action.GetBindingDisplayString(bindingIndex);
                _toast?.Show($"{action.name} -> {display}", 1.6f);
            });

        _op.Start();
    }

    /// <summary>
    /// Cleanup after a rebind finishes/cancels:
    /// - dispose operation
    /// - restore UI input mode
    /// - restore action enabled state
    /// </summary>
    private void CleanupAfterRebind(InputAction action)
    {
        _op?.Dispose();
        _op = null;

        // Restore UI mode. (Your InputMode stack will keep UI active anyway.)
        _input.EnableUI();

        if (_wasActionEnabled) action.Enable();
        if (statusText) statusText.text = "";
    }

    /// <summary>
    /// Saves all binding overrides into SettingsData.controls.bindingOverridesJson.
    /// Then marks dirty and saves settings immediately.
    ///
    /// Note:
    /// - For template projects, saving immediately is simplest and safest.
    /// - If you want Apply/Cancel for controls, you would instead only MarkDirty and save on Apply.
    /// </summary>
    private void SaveOverridesToSettings()
    {
        var json = _input.Actions.SaveBindingOverridesAsJson();
        _settings.Data.controls.bindingOverridesJson = json;
        _settings.MarkDirty();
        _settings.Save();
    }

    /// <summary>
    /// Resets the binding override for this action/bindingIndex only.
    /// </summary>
    private void ResetBinding()
    {
        CancelIfRunning();

        var action = GetAction();
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;

        action.RemoveBindingOverride(bindingIndex);

        SaveOverridesToSettings();
        RefreshUI();

        var display = action.GetBindingDisplayString(bindingIndex);
        _toast?.Show($"{action.name} reset ({display})", 1.4f);
    }

    /// <summary>
    /// Cancels an active rebinding operation if it is running.
    /// </summary>
    private void CancelIfRunning()
    {
        if (_op == null) return;

        _op.Cancel();
        _op.Dispose();
        _op = null;

        _input.EnableUI();
        if (statusText) statusText.text = "";
    }

    /// <summary>
    /// Safe UI update helper (avoids null refs).
    /// </summary>
    private void SafeSetTexts(string actionName, string binding)
    {
        if (actionNameText) actionNameText.text = actionName;
        if (bindingText) bindingText.text = binding;
        if (statusText) statusText.text = "";
    }
}
