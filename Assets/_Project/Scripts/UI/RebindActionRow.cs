using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;

public class RebindActionRow : MonoBehaviour
{
    [Header("Binding")]
    [Tooltip("Example: Gameplay/Pause")]
    [SerializeField] private string actionPath = "Gameplay/Pause";
    [SerializeField] private int bindingIndex = 0;

    [SerializeField] private InputActionAsset actionsAsset;

    [Header("UI")]
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private Button rebindButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text statusText;

    private IInputService _input;
    private ISettingsService _settings;

    private InputActionRebindingExtensions.RebindingOperation _op;
    private bool _wasActionEnabled;

    private void OnEnable()
    {
        _input = ServiceContainer.Get<IInputService>();
        _settings = ServiceContainer.Get<ISettingsService>();

        rebindButton.onClick.AddListener(StartRebind);
        resetButton.onClick.AddListener(ResetBinding);

        RefreshUI();
    }

    private void OnDisable()
    {
        rebindButton.onClick.RemoveListener(StartRebind);
        resetButton.onClick.RemoveListener(ResetBinding);
        CancelIfRunning();
    }

    private InputAction GetAction()
    {
        if (_input?.Actions == null) return null;
        if (string.IsNullOrWhiteSpace(actionPath)) return null;
        return _input.Actions.FindAction(actionPath, throwIfNotFound: false);
    }

    public void RefreshUI()
    {
        var action = GetAction();
        if (action == null)
        {
            Debug.LogError($"[RebindActionRow] Action not found: '{actionPath}' on '{gameObject.name}'.");
            if (actionNameText) actionNameText.text = "(Missing Action)";
            if (bindingText) bindingText.text = "(N/A)";
            if (statusText) statusText.text = "";
            return;
        }

        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count)
        {
            Debug.LogError($"[RebindActionRow] Invalid bindingIndex={bindingIndex} for '{actionPath}'. Bindings={action.bindings.Count}");
            if (actionNameText) actionNameText.text = action.name;
            if (bindingText) bindingText.text = "(Invalid Index)";
            if (statusText) statusText.text = "";
            return;
        }

        if (actionNameText) actionNameText.text = action.name;
        if (bindingText) bindingText.text = action.GetBindingDisplayString(bindingIndex);
        if (statusText) statusText.text = "";
    }

    private void StartRebind()
    {
        CancelIfRunning();

        var action = GetAction();
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;

        _wasActionEnabled = action.enabled;
        action.Disable();

        if (statusText) statusText.text = "Press a key... (ESC to cancel)";
        if (bindingText) bindingText.text = "...";

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
                SaveOverridesToSettings();
                RefreshUI();
            });

        _op.Start();
    }

    private void CleanupAfterRebind(InputAction action)
    {
        _op?.Dispose();
        _op = null;

        _input.EnableUI();

        if (_wasActionEnabled) action.Enable();
        if (statusText) statusText.text = "";
    }

    private void SaveOverridesToSettings()
    {
        var json = _input.Actions.SaveBindingOverridesAsJson();
        _settings.Data.controls.bindingOverridesJson = json;
        _settings.MarkDirty();
        _settings.Save();
    }

    private void ResetBinding()
    {
        CancelIfRunning();

        var action = GetAction();
        if (action == null) return;
        if (bindingIndex < 0 || bindingIndex >= action.bindings.Count) return;

        action.RemoveBindingOverride(bindingIndex);
        SaveOverridesToSettings();
        RefreshUI();
    }

    private void CancelIfRunning()
    {
        if (_op == null) return;
        _op.Cancel();
        _op.Dispose();
        _op = null;

        _input.EnableUI();
        if (statusText) statusText.text = "";
    }
}
