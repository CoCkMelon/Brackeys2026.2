using UnityEngine.InputSystem;
using ZXTemplate.Core;
using ZXTemplate.Input;

namespace ZXTemplate.Settings
{
    /// <summary>
    /// Applies ControlsSettings (binding overrides) to the runtime InputActionAsset.
    ///
    /// How it works:
    /// - Clear all overrides first (ensures we fully match saved state).
    /// - If JSON exists, load overrides via InputSystem's built-in serialization.
    ///
    /// Data source:
    /// - ControlsSettings.bindingOverridesJson
    ///   saved from InputActionAsset.SaveBindingOverridesAsJson()
    ///
    /// Important:
    /// - This does NOT change action map enable states (Gameplay/UI). That's handled by InputModeService.
    /// </summary>
    public class SettingsApplierControls
    {
        public static void Apply(ControlsSettings c)
        {
            var input = ServiceContainer.Get<IInputService>();
            var actions = input.Actions;

            // Reset to default bindings first.
            actions.RemoveAllBindingOverrides();

            // Apply saved overrides.
            if (!string.IsNullOrEmpty(c.bindingOverridesJson))
            {
                actions.LoadBindingOverridesFromJson(c.bindingOverridesJson);
            }
        }
    }
}
