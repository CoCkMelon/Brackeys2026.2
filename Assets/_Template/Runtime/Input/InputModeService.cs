using System.Collections.Generic;

namespace ZXTemplate.Input
{
    /// <summary>
    /// Controls which input map is currently active (Gameplay vs UI).
    ///
    /// Why this exists:
    /// - Many systems may want to switch input mode temporarily (Pause menu, Settings, Console, etc.).
    /// - We avoid "who owns the input?" conflicts by using a token (handle) stack:
    ///     - Acquire(UI) when a menu opens
    ///     - Release(token) when it closes
    ///   The most recently acquired token wins ("last acquired wins").
    ///
    /// BaseMode:
    /// - The default mode when no tokens are held (typically Gameplay).
    ///
    /// Important:
    /// - Always Release() the token you acquired.
    /// - If you forget to release, input may stay in UI mode forever.
    /// </summary>
    public class InputModeService : IInputModeService
    {
        private readonly IInputService _input;

        // Default mode when no tokens are held.
        private InputMode _baseMode = InputMode.Gameplay;

        // Token id generator (monotonic).
        private long _nextId = 1;

        // Active tokens (acts like a stack; last acquired wins).
        private readonly List<Handle> _handles = new();

        public InputMode BaseMode => _baseMode;

        /// <summary>
        /// The effective mode after considering base mode + active tokens.
        /// </summary>
        public InputMode CurrentMode => ComputeEffectiveMode();

        public InputModeService(IInputService input)
        {
            _input = input;
            Apply(CurrentMode);
        }

        /// <summary>
        /// Sets the base mode (used when there are no active tokens).
        /// Example: during gameplay scene start you may set base to Gameplay,
        /// while in main menu you might set base to UI.
        /// </summary>
        public void SetBaseMode(InputMode mode)
        {
            _baseMode = mode;
            Apply(CurrentMode);
        }

        /// <summary>
        /// Acquire an input mode token.
        /// The returned token must be passed back to Release().
        /// "reason" is only for debugging / readability.
        /// </summary>
        public object Acquire(InputMode mode, string reason)
        {
            var token = new Handle(_nextId++, mode, reason);
            _handles.Add(token);
            Apply(CurrentMode);
            return token;
        }

        /// <summary>
        /// Releases a previously acquired token.
        /// If token is invalid or already released, this is a no-op.
        /// </summary>
        public void Release(object token)
        {
            if (token is not Handle h) return;

            // Remove by id (safe even if token is not the latest).
            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                if (_handles[i].Id == h.Id)
                {
                    _handles.RemoveAt(i);
                    break;
                }
            }

            Apply(CurrentMode);
        }

        /// <summary>
        /// Computes the effective mode.
        /// Rule: last acquired wins. If no tokens, use base mode.
        /// </summary>
        private InputMode ComputeEffectiveMode()
        {
            if (_handles.Count == 0) return _baseMode;
            return _handles[^1].Mode;
        }

        /// <summary>
        /// Applies the mode to the underlying input system (enable/disable action maps).
        /// </summary>
        private void Apply(InputMode mode)
        {
            if (mode == InputMode.UI) _input.EnableUI();
            else _input.EnableGameplay();
        }

        /// <summary>
        /// Token object representing a temporary input mode override.
        /// Kept private to prevent external mutation.
        /// </summary>
        private sealed class Handle
        {
            public long Id { get; }
            public InputMode Mode { get; }
            public string Reason { get; }

            public Handle(long id, InputMode mode, string reason)
            {
                Id = id;
                Mode = mode;
                Reason = reason;
            }
        }
    }
}
