namespace ZXTemplate.Input
{
    /// <summary>
    /// Manages the current input mode (Gameplay vs UI) using a token-based override system.
    ///
    /// Why token-based:
    /// - Multiple UI layers may request UI mode (Pause menu, Settings, Console, etc.).
    /// - The game should remain in UI mode until ALL UI layers release their tokens.
    ///
    /// Rules:
    /// - BaseMode is the default when no tokens are held (typically Gameplay).
    /// - Acquire() returns a token; the caller MUST Release(token) later.
    /// - Last acquired token wins when multiple tokens are held (LIFO behavior).
    /// </summary>
    public interface IInputModeService
    {
        /// <summary>Default mode when there are no active tokens.</summary>
        InputMode BaseMode { get; }

        /// <summary>Effective mode after applying token overrides.</summary>
        InputMode CurrentMode { get; }

        /// <summary>Sets the default mode used when no tokens are held.</summary>
        void SetBaseMode(InputMode mode);

        /// <summary>
        /// Acquires a mode override token.
        /// "reason" is for debugging/readability only.
        /// </summary>
        object Acquire(InputMode mode, string reason);

        /// <summary>
        /// Releases a previously acquired token. No-op if token is null/unknown.
        /// </summary>
        void Release(object token);
    }
}
