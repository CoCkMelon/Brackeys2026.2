using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZXTemplate.Input
{
    public class InputService : IInputService
    {
        public InputActionAsset Actions { get; }

        private readonly InputAction _move;
        private readonly InputAction _pause;

        public Vector2 Move => _move.ReadValue<Vector2>();
        public event Action OnPause;

        public InputService(InputActionAsset actions)
        {
            _move = actions.FindAction("Gameplay/Move", throwIfNotFound: false);
            _pause = actions.FindAction("Gameplay/Pause", throwIfNotFound: false);

            _pause.performed += _ => OnPause?.Invoke();

            Actions = actions;
            Actions.Enable();
        }

        public void EnableGameplay()
        {
            Actions.FindActionMap("UI", false)?.Disable();
            Actions.FindActionMap("Gameplay", false)?.Enable();
        }

        public void EnableUI()
        {
            Actions.FindActionMap("Gameplay", false)?.Disable();
            Actions.FindActionMap("UI", false)?.Enable();
        }
    }
}
