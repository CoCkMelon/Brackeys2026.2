using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZXTemplate.Input
{
    /// <summary>
    /// Default input implementation for the template.
    ///
    /// Responsibilities:
    /// - Hold a reference to the InputActionAsset used at runtime.
    /// - Provide commonly used actions as simple API:
    ///     - Move: Vector2
    ///     - Pause: event
    /// - Switch between Gameplay/UI action maps (EnableGameplay/EnableUI).
    ///
    /// Notes / Conventions:
    /// - The template expects an action map named "Gameplay" and one named "UI".
    /// - Common actions are expected at:
    ///     - "Gameplay/Move"
    ///     - "Gameplay/Pause"
    ///
    /// Important:
    /// - This class does NOT call Actions.Enable() globally.
    ///   Instead, InputModeService controls which maps are enabled.
    /// </summary>
    public class InputService : IInputService
    {
        /// <summary>Runtime InputActionAsset reference (used by rebinding UI).</summary>
        public InputActionAsset Actions { get; private set; }

        // Cached action references (faster and safer than searching every frame).
        private readonly InputAction _move;
        private readonly InputAction _pause;

        /// <summary>Reads current Move value. If _move is null, returns Vector2.zero.</summary>
        public Vector2 Move => _move != null ? _move.ReadValue<Vector2>() : Vector2.zero;

        /// <summary>Raised when Pause is performed.</summary>
        public event Action OnPause;

        public InputService(InputActionAsset actions)
        {
            Actions = actions;

            // FindAction("Map/Action") returns null if not found when throwIfNotFound=false.
            _move = actions != null ? actions.FindAction("Gameplay/Move", throwIfNotFound: false) : null;
            _pause = actions != null ? actions.FindAction("Gameplay/Pause", throwIfNotFound: false) : null;

            // Bind event only if action exists (prevents NullReference in misconfigured projects).
            if (_pause != null)
                _pause.performed += _ => OnPause?.Invoke();
        }

        /// <summary>
        /// Enables Gameplay action map and disables UI map.
        /// This is typically used when gameplay is active.
        /// </summary>
        public void EnableGameplay()
        {
            if (Actions == null) return;

            Actions.FindActionMap("UI", false)?.Disable();
            Actions.FindActionMap("Gameplay", false)?.Enable();
        }

        /// <summary>
        /// Enables UI action map and disables Gameplay map.
        /// This is typically used when a menu is open.
        /// </summary>
        public void EnableUI()
        {
            if (Actions == null) return;

            Actions.FindActionMap("Gameplay", false)?.Disable();
            Actions.FindActionMap("UI", false)?.Enable();
        }
    }
}
