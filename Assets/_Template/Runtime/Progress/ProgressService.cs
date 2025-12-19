using System;
using ZXTemplate.Save;

namespace ZXTemplate.Progress
{
    public class ProgressService : IProgressService
    {
        public ProgressData Data { get; private set; }
        public event Action OnChanged;

        private readonly ISaveService _save;

        public ProgressService(ISaveService save)
        {
            _save = save;
            LoadOrCreate();
        }

        private void LoadOrCreate()
        {
            if (!_save.TryLoad(ProgressKeys.Main, out ProgressData data) || data == null || !data.initialized)
            {
                Data = new ProgressData();
                _save.Save(ProgressKeys.Main, Data);
                return;
            }

            // Migration hook（以后版本升级时用）
            if (data.version != ProgressData.CurrentVersion)
            {
                // 目前 v1 没有迁移逻辑；你以后改结构就在这里做。
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
            if (score > Data.highScore)
            {
                Data.highScore = score;
                ChangedAndSave();
            }
        }

        public void UnlockLevel(int level)
        {
            if (level > Data.unlockedLevel)
            {
                Data.unlockedLevel = level;
                ChangedAndSave();
            }
        }

        public int GetInt(string key, int defaultValue = 0) => Data.GetInt(key, defaultValue);

        public void SetInt(string key, int value)
        {
            Data.SetInt(key, value);
            ChangedAndSave();
        }

        public void Save()
        {
            Data.Clamp();
            _save.Save(ProgressKeys.Main, Data);
        }

        public void Reset()
        {
            Data = new ProgressData();
            _save.Save(ProgressKeys.Main, Data);
            OnChanged?.Invoke();
        }

        private void ChangedAndSave()
        {
            Save();
            OnChanged?.Invoke();
        }
    }
}
