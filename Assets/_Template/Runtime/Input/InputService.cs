using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZXTemplate.Input
{
    public class InputService : IInputService
    {
        public InputActionAsset Actions { get; private set; }

        private readonly InputAction _move;
        private readonly InputAction _pause;

        public Vector2 Move => _move.ReadValue<Vector2>();
        public event Action OnPause;

        public InputService(InputActionAsset actions)
        {
            _move = actions.FindAction("Gameplay/Move", throwIfNotFound: false);
            _pause = actions.FindAction("Gameplay/Pause", throwIfNotFound: false);

            //Debug.Log($"[InputService] Move action = {(_move != null ? _move.name : "NULL")}");
            //Debug.Log($"[InputService] Pause action = {(_pause != null ? _pause.name : "NULL")}");


            _pause.performed += _ => OnPause?.Invoke();

            Actions = actions;
            //Debug.Log($"[InputService] Actions asset = {(Actions ? Actions.name : "NULL")}, maps={(Actions ? Actions.actionMaps.Count : 0)}");

            //Actions.Enable();
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
