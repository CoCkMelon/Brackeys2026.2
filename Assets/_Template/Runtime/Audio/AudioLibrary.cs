using System;
using System.Collections.Generic;
using UnityEngine;

namespace ZXTemplate.Audio
{
    /// <summary>
    /// A ScriptableObject database mapping string ids to AudioClips.
    ///
    /// Implementation:
    /// - Builds a dictionary cache for O(1) lookup.
    /// - Logs warnings for duplicate ids (first entry wins).
    /// </summary>
    [CreateAssetMenu(menuName = "ZXTemplate/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        [Tooltip("List of audio entries. Ids should be unique.")]
        public ClipEntry[] clips;

        [Serializable]
        public struct ClipEntry
        {
            public string id;
            public AudioClip clip;
        }

        // Runtime cache (id -> clip)
        private Dictionary<string, AudioClip> _cache;

        private void OnEnable()
        {
            RebuildCache();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keep cache fresh while editing the asset in Inspector.
            RebuildCache();
        }
#endif

        /// <summary>
        /// Finds an AudioClip by id. Returns null if not found.
        /// </summary>
        public AudioClip Find(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_cache == null) RebuildCache();

            return _cache.TryGetValue(id, out var clip) ? clip : null;
        }

        /// <summary>
        /// Rebuilds the dictionary cache from the clips array.
        /// Duplicate ids will be ignored (first wins) and a warning is logged.
        /// </summary>
        private void RebuildCache()
        {
            _cache = new Dictionary<string, AudioClip>(StringComparer.Ordinal);

            if (clips == null) return;

            for (int i = 0; i < clips.Length; i++)
            {
                var id = clips[i].id;
                var clip = clips[i].clip;

                if (string.IsNullOrEmpty(id)) continue;

                if (_cache.ContainsKey(id))
                {
                    Debug.LogWarning($"[AudioLibrary] Duplicate id '{id}' in '{name}'. First entry will be used.");
                    continue;
                }

                _cache[id] = clip;
            }
        }
    }
}
