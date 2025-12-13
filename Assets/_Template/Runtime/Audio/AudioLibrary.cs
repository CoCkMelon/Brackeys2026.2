using System;
using UnityEngine;

namespace ZXTemplate.Audio
{
    [CreateAssetMenu(menuName = "ZXTemplate/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        public ClipEntry[] clips;

        [Serializable]
        public struct ClipEntry
        {
            public string id;
            public AudioClip clip;
        }

        public AudioClip Find(string id)
        {
            foreach (var e in clips)
                if (e.id == id) return e.clip;
            return null;
        }
    }
}
