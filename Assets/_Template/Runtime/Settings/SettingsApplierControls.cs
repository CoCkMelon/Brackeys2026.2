using UnityEngine.InputSystem;
using ZXTemplate.Core;
using ZXTemplate.Input;

namespace ZXTemplate.Settings
{
    public class SettingsApplierControls
    {
        public static void Apply(ControlsSettings c)
        {
            var input = ServiceContainer.Get<IInputService>();
            var actions = input.Actions;

            actions.RemoveAllBindingOverrides();

            if (!string.IsNullOrEmpty(c.bindingOverridesJson))
            {
                actions.LoadBindingOverridesFromJson(c.bindingOverridesJson);
            }
        }
    }
}
