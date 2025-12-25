using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Progress
{
    /// <summary>
    /// Serializable player progress data (persisted by ProgressService).
    ///
    /// Goals:
    /// - Provide a few "universal" fields that most course projects can reuse:
    ///     - coins, highScore, unlockedLevel
    /// - Provide a lightweight extensible key-value store for additional integers.
    ///
    /// Serialization note:
    /// - Unity JsonUtility does not support Dictionary well by default.
    /// - We use List<IntVar> so it can be serialized safely.
    ///
    /// Versioning:
    /// - Keep a version number for future migrations when the schema changes.
    /// </summary>
    [Serializable]
    public class ProgressData
    {
        public const int CurrentVersion = 1;

        /// <summary>
        /// True if the save is initialized. Used to detect first-run / corrupted saves.
        /// </summary>
        public bool initialized = true;

        /// <summary>
        /// Schema version of this save file (used for migration).
        /// </summary>
        public int version = CurrentVersion;

        // Universal fields: likely useful for most course projects.
        [Min(0)] public int coins = 0;
        [Min(0)] public int highScore = 0;
        [Min(1)] public int unlockedLevel = 1;

        /// <summary>
        /// Extensible integer variables. Acts like a small key-value store.
        /// Example keys: "tutorial_done", "level2_stars", "upgrade_drill_level".
        /// </summary>
        public List<IntVar> ints = new();

        [Serializable]
        public struct IntVar
        {
            public string key;
            public int value;
        }

        /// <summary>
        /// Clamps fields to valid ranges. Call before applying gameplay logic or saving.
        /// </summary>
        public void Clamp()
        {
            coins = Mathf.Max(0, coins);
            highScore = Mathf.Max(0, highScore);
            unlockedLevel = Mathf.Max(1, unlockedLevel);
        }

        /// <summary>
        /// Gets an int value by key; returns defaultValue if not found.
        /// </summary>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;

            for (int i = 0; i < ints.Count; i++)
                if (ints[i].key == key) return ints[i].value;

            return defaultValue;
        }

        /// <summary>
        /// Sets an int value by key (adds if missing, replaces if exists).
        /// </summary>
        public void SetInt(string key, int value)
        {
            if (string.IsNullOrEmpty(key)) return;

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
