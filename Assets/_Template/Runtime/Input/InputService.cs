using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZXTemplate.Input
{
    public class InputService : IInputService
    {
        private readonly InputActionAsset _asset;

        private readonly InputAction _move;
        private readonly InputAction _pause;

        public Vector2 Move => _move.ReadValue<Vector2>();
        public event Action OnPause;

        public InputService(InputActionAsset asset)
        {
            _asset = asset;

            // Ô¼¶¨£ºActionMap = "Gameplay"£¬Action = "Move" / "Pause"
            _move = _asset.FindAction("Gameplay/Move", throwIfNotFound: false);
            _pause = _asset.FindAction("Gameplay/Pause", throwIfNotFound: false);

            _pause.performed += _ => OnPause?.Invoke();

            _asset.Enable();
        }

        public void EnableGameplay()
        {
            _asset.FindActionMap("UI", false)?.Disable();
            _asset.FindActionMap("Gameplay", false)?.Enable();
        }

        public void EnableUI()
        {
            _asset.FindActionMap("Gameplay", false)?.Disable();
            _asset.FindActionMap("UI", false)?.Enable();
        }
    }
}
