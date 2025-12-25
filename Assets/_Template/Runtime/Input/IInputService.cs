using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZXTemplate.Input
{
    /// <summary>
    /// High-level input API used by gameplay and UI systems.
    ///
    /// This service:
    /// - Exposes commonly used actions as simple properties/events (Move, Pause).
    /// - Provides access to the underlying InputActionAsset for advanced UI (rebinding).
    /// - Controls which ActionMap is enabled (Gameplay vs UI).
    ///
    /// Notes:
    /// - Action names (e.g. "Gameplay/Move") are conventions in the InputActions asset.
    /// - For rebinding UI, use Actions.FindAction("Map/Action") and apply overrides.
    /// </summary>
    public interface IInputService
    {
        /// <summary>Current movement vector read from the Move action.</summary>
        Vector2 Move { get; }

        /// <summary>Raised when the Pause action is performed.</summary>
        event Action OnPause;

        /// <summary>The InputActionAsset used at runtime (required for rebinding UI).</summary>
        InputActionAsset Actions { get; }

        /// <summary>Enables Gameplay action map and disables UI map.</summary>
        void EnableGameplay();

        /// <summary>Enables UI action map and disables Gameplay map.</summary>
        void EnableUI();
    }
}
