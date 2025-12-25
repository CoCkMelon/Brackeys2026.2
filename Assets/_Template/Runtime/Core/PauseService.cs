using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Core
{
    /// <summary>
    /// Token-based pause implementation.
    ///
    /// Behavior:
    /// - When the first token is acquired: Time.timeScale -> 0 (paused)
    /// - When the last token is released: Time.timeScale -> 1 (unpaused)
    ///
    /// Why token-based:
    /// - Prevents "pause ownership" bugs when multiple UI layers can pause the game
    ///   (Pause menu, Settings, Console, Upgrade screen, etc.).
    ///
    /// Notes:
    /// - This implementation only changes Time.timeScale.
    ///   It does NOT automatically pause AudioListener or mute audio.
    ///   If you want audio to keep playing during pause, set AudioMixer.updateMode to UnscaledTime.
    /// </summary>
    public class PauseService : IPauseService
    {
        // Each Acquire() returns a unique token. We store them in a set.
        private readonly HashSet<object> _tokens = new();

        /// <summary>True if one or more pause tokens are currently held.</summary>
        public bool IsPaused => _tokens.Count > 0;

        /// <summary>
        /// Acquires a pause token. Caller must Release() it.
        /// </summary>
        public object Acquire(string reason)
        {
            var token = new PauseToken(reason);
            _tokens.Add(token);
            Apply();
            return token;
        }

        /// <summary>
        /// Releases a pause token. No-op for null/unknown tokens.
        /// </summary>
        public void Release(object token)
        {
            if (token == null) return;

            _tokens.Remove(token);
            Apply();
        }

        /// <summary>
        /// Applies pause state to Unity time scale.
        /// </summary>
        private void Apply()
        {
            Time.timeScale = IsPaused ? 0f : 1f;
        }

        /// <summary>
        /// Token object. Contains a reason string for debugging.
        /// Kept private to prevent external mutation.
        /// </summary>
        private class PauseToken
        {
            public readonly string Reason;
            public PauseToken(string reason) => Reason = reason;
        }
    }
}
