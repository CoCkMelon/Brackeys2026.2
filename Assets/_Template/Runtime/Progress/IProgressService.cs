using System;

namespace ZXTemplate.Progress
{
    /// <summary>
    /// High-level progress API for small projects / templates.
    ///
    /// What "progress" means here:
    /// - Player meta data that persists across sessions (coins, high score, unlocked level, etc.)
    /// - A tiny key-value store for custom ints (for quick course project iteration)
    ///
    /// Notes:
    /// - This service is intentionally simple (single local save slot).
    /// - Changes typically auto-save immediately (see ProgressService implementation).
    /// </summary>
    public interface IProgressService
    {
        /// <summary>Current progress data in memory.</summary>
        ProgressData Data { get; }

        /// <summary>Raised after data changes (and after save, depending on implementation).</summary>
        event Action OnChanged;

        void AddCoins(int amount);
        bool TrySpendCoins(int amount);

        void SetHighScoreIfBetter(int score);
        void UnlockLevel(int level);

        /// <summary>
        /// Gets a custom integer value by key, or defaultValue if missing.
        /// Useful for quick feature flags/unlocks without changing ProgressData schema.
        /// </summary>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// Sets a custom integer value by key.
        /// </summary>
        void SetInt(string key, int value);

        /// <summary>Forces saving current progress data to disk.</summary>
        void Save();

        /// <summary>Resets progress to defaults and saves immediately.</summary>
        void Reset();
    }
}
