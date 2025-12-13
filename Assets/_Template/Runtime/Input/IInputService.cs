using System;
using UnityEngine;

namespace ZXTemplate.Input
{
    public interface IInputService
    {
        Vector2 Move { get; }
        event Action OnPause;
        void EnableGameplay();
        void EnableUI();
    }
}
