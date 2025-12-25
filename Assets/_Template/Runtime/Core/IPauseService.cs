namespace ZXTemplate.Core
{
    /// <summary>
    /// Central pause controller for the game.
    ///
    /// Design:
    /// - Token-based (stack/handle style): multiple systems can request pause safely.
    /// - Game is considered paused if at least one token is held.
    ///
    /// Example:
    /// - Pause menu opens -> Acquire("PauseMenu")
    /// - Settings opens -> Acquire("Settings")
    /// - Closing settings releases its token, but game stays paused if pause menu token still exists.
    /// </summary>
    public interface IPauseService
    {
        /// <summary>True if one or more pause tokens are currently held.</summary>
        bool IsPaused { get; }

        /// <summary>
        /// Requests pause and returns a token that must be released later.
        /// "reason" is only for debugging/readability.
        /// </summary>
        object Acquire(string reason);

        /// <summary>
        /// Releases a previously acquired token. No-op if token is null/unknown.
        /// </summary>
        void Release(object token);
    }
}
