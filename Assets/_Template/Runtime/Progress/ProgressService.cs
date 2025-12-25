using System;
using ZXTemplate.Save;

namespace ZXTemplate.Progress
{
    /// <summary>
    /// Default progress implementation that persists ProgressData via ISaveService.
    ///
    /// Behavior:
    /// - Loads on construction; creates defaults on first run.
    /// - Most mutating methods save immediately (ChangedAndSave).
    ///
    /// Why auto-save:
    /// - For course projects/templates, it reduces the chance of losing progress.
    /// - Progress data is usually small, so saving often is acceptable.
    ///
    /// Migration:
    /// - A version field exists in ProgressData for future schema changes.
    /// </summary>
    public class ProgressService : IProgressService, ISaveParticipant
    {
        public ProgressData Data { get; private set; }
        public event Action OnChanged;

        private readonly ISaveService _save;

        public ProgressService(ISaveService save)
        {
            _save = save;
            LoadOrCreate();
        }

        /// <summary>
        /// Loads progress from storage; if missing/invalid, creates defaults and saves them.
        /// </summary>
        private void LoadOrCreate()
        {
            if (!_save.TryLoad(ProgressKeys.Main, out ProgressData data) || data == null || !data.initialized)
            {
                Data = new ProgressData();
                _save.Save(ProgressKeys.Main, Data);
                return;
            }

            // Migration hook: use this block when you change ProgressData structure in future versions.
            if (data.version != ProgressData.CurrentVersion)
            {
                // v1 has no migrations yet.
                data.version = ProgressData.CurrentVersion;
            }

            data.Clamp();
            Data = data;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Data.coins += amount;
            ChangedAndSave();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (Data.coins < amount) return false;

            Data.coins -= amount;
            ChangedAndSave();
            return true;
        }

        public void SetHighScoreIfBetter(int score)
        {
            if (score <= Data.highScore) return;

            Data.highScore = score;
            ChangedAndSave();
        }

        public void UnlockLevel(int level)
        {
            if (level <= Data.unlockedLevel) return;

            Data.unlockedLevel = level;
            ChangedAndSave();
        }

        public int GetInt(string key, int defaultValue = 0) => Data.GetInt(key, defaultValue);

        public void SetInt(string key, int value)
        {
            Data.SetInt(key, value);
            ChangedAndSave();
        }

        /// <summary>
        /// Saves current progress data (even if nothing changed).
        /// Participants should be safe to call multiple times.
        /// </summary>
        public void Save()
        {
            Data.Clamp();
            _save.Save(ProgressKeys.Main, Data);
        }

        /// <summary>
        /// Resets progress to defaults and saves immediately.
        /// </summary>
        public void Reset()
        {
            Data = new ProgressData();
            _save.Save(ProgressKeys.Main, Data);
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Convenience helper: persist then notify listeners.
        /// </summary>
        private void ChangedAndSave()
        {
            Save();
            OnChanged?.Invoke();
        }
    }
}
