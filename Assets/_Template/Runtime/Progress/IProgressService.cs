using System;

namespace ZXTemplate.Progress
{
    public interface IProgressService
    {
        ProgressData Data { get; }
        event Action OnChanged;

        void AddCoins(int amount);
        bool TrySpendCoins(int amount);

        void SetHighScoreIfBetter(int score);
        void UnlockLevel(int level);

        int GetInt(string key, int defaultValue = 0);
        void SetInt(string key, int value);

        void Save();
        void Reset();
    }
}
