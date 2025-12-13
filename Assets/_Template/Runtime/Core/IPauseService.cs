using UnityEngine;

namespace ZXTemplate.Core
{
    public interface IPauseService
    {
        bool IsPaused { get; }
        object Acquire(string reason);
        void Release(object token);
    }
}
