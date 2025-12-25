using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;
using ZXTemplate.UI;

public class RebindActionRow : MonoBehaviour
{
    [Header("Binding")]
    [Tooltip("Example: Gameplay/Jump")]
    [SerializeField] private string actionPath = "Gameplay/Jump";
    [Tooltip("Which binding of the action to rebind (usually keyboard is 0)")]
    [SerializeField] private int bindingIndex = 0;

    // 仅用于编辑器下拉（不参与运行逻辑）
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
        //Debug.Log($"[ASM] RebindRow ServiceContainer = {typeof(ZXTemplate.Core.ServiceContainer).Assembly.FullName}");
        //Debug.Log($"[ASM] RebindRow IInputService    = {typeof(ZXTemplate.Input.IInputService).Assembly.FullName}");

        //var input = ZXTemplate.Core.ServiceContainer.Get<ZXTemplate.Input.IInputService>();
        //Debug.Log($"[RebindRow] inputType={input.GetType().FullName} asm={input.GetType().Assembly.FullName} actions={(input.Actions ? input.Actions.name : "NULL")}");



        _input = ServiceContainer.Get<IInputService>();
        _settings = ServiceContainer.Get<ISettingsService>();
        _toast = ServiceContainer.Get<IToastService>();

        if (rebindButton) rebindButton.onClick.AddListener(StartRebind);
        if (resetButton) resetButton.onClick.AddListener(ResetBinding);

        //Debug.Log($"[RebindRow] IInputService instance = {_input.GetType().FullName} id={_input.GetHashCode()} actions={(_input.Actions ? _input.Actions.name : "NULL")}");

        RefreshUI();
    }

    private void OnDisable()
    {
        if (rebindButton) rebindButton.onClick.RemoveListener(StartRebind);
        if (resetButton) resetButton.onClick.RemoveListener(ResetBinding);

        CancelIfRunning();
    }

    private InputAction GetAction()
    {
        if (_input == null)
            ServiceContainer.TryGet(out _input);

        var asset = _input?.Actions;
        if (asset == null) return null;

        return asset.FindAction(actionPath, throwIfNotFound: false);
    }


    public void RefreshUI()
    {
        if (_input?.Actions == null)
        {
            Debug.LogError($"[RebindActionRow] Runtime IInputService.Actions is NULL on '{gameObject.name}'. Check Bootstrapper inputActions assignment and InputService constructor.");
            SafeSetTexts("(Input NULL)", "(N/A)");
            return;
        }

        if (actionsAsset != null && actionsAsset != _input.Actions)
        {
            Debug.LogWarning($"[RebindActionRow] Mismatch InputActionAsset on '{gameObject.name}'. " +
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
            Debug.LogError($"[RebindActionRow] Invalid bindingIndex={bindingIndex} for '{actionPath}'. Bindings={action.bindings.Count} on '{gameObject.name}'.");
            SafeSetTexts(action.name, "(Invalid Index)");
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

        // 不建议给 composite 重绑（Move 2DVector），你已经决定不做
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

        // 禁用整个 actions，避免 UI 导航/点击干扰
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

                var display = action.GetBindingDisplayString(bindingIndex);
                _toast?.Show($"{action.name} -> {display}", 1.6f);
            });

        _op.Start();
    }

    private void CleanupAfterRebind(InputAction action)
    {
        _op?.Dispose();
        _op = null;

        // 恢复 UI 输入（你的 InputModeStack 会维持 UI）
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

        var display = action.GetBindingDisplayString(bindingIndex);
        _toast?.Show($"{action.name} reset ({display})", 1.4f);
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

    private void SafeSetTexts(string actionName, string binding)
    {
        if (actionNameText) actionNameText.text = actionName;
        if (bindingText) bindingText.text = binding;
        if (statusText) statusText.text = "";
    }
}
