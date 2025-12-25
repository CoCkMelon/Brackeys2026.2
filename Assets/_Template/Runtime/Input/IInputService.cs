using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ZXTemplate.Input
{
    public interface IInputService
    {
        Vector2 Move { get; }
        event Action OnPause;
        InputActionAsset Actions { get; }
        void EnableGameplay();
        void EnableUI();
    }
}
