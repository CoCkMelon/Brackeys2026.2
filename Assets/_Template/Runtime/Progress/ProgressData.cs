using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Progress
{
    [Serializable]
    public class ProgressData
    {
        public const int CurrentVersion = 1;

        public bool initialized = true;
        public int version = CurrentVersion;

        // 最通用的字段：你几乎所有课程项目都用得到
        [Min(0)] public int coins = 0;
        [Min(0)] public int highScore = 0;
        [Min(1)] public int unlockedLevel = 1;

        // 可扩展：用 List 代替 Dictionary（JsonUtility 兼容）
        public List<IntVar> ints = new();

        [Serializable]
        public struct IntVar
        {
            public string key;
            public int value;
        }

        public void Clamp()
        {
            coins = Mathf.Max(0, coins);
            highScore = Mathf.Max(0, highScore);
            unlockedLevel = Mathf.Max(1, unlockedLevel);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            for (int i = 0; i < ints.Count; i++)
                if (ints[i].key == key) return ints[i].value;
            return defaultValue;
        }

        public void SetInt(string key, int value)
        {
            for (int i = 0; i < ints.Count; i++)
            {
                if (ints[i].key == key)
                {
                    ints[i] = new IntVar { key = key, value = value };
                    return;
                }
            }
            ints.Add(new IntVar { key = key, value = value });
        }
    }
}
