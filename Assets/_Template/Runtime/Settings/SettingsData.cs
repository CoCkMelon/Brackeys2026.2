using System;
using UnityEngine;

namespace ZXTemplate.Settings
{
    [Serializable]
    public class SettingsData
    {
        public const int CurrentVersion = 1;

        public bool initialized = true;
        public int version = CurrentVersion;

        public AudioSettings audio = new();
        public VideoSettings video = new();
        public ControlsSettings controls = new();

        public void Clamp()
        {
            audio.Clamp();
            video.Clamp();
        }
    }

    [Serializable]
    public class AudioSettings
    {
        [Range(0f, 1f)] public float master = 1f;
        [Range(0f, 1f)] public float bgm = 1f;
        [Range(0f, 1f)] public float sfx = 1f;

        public bool masterMuted = false;
        public bool bgmMuted = false;
        public bool sfxMuted = false;

        public void Clamp()
        {
            master = Mathf.Clamp01(master);
            bgm = Mathf.Clamp01(bgm);
            sfx = Mathf.Clamp01(sfx);
        }
    }

    [Serializable]
    public class VideoSettings
    {
        public bool fullscreen = true;

        // 用“当前选择的分辨率”保存
        public int width = 1920;
        public int height = 1080;

        // 对齐 QualitySettings.names
        public int qualityIndex = 0;

        public void Clamp()
        {
            width = Mathf.Max(640, width);
            height = Mathf.Max(360, height);

            var maxQ = Mathf.Max(0, QualitySettings.names.Length - 1);
            qualityIndex = Mathf.Clamp(qualityIndex, 0, maxQ);
        }
    }

    [Serializable]
    public class ControlsSettings
    {
        public string bindingOverridesJson = "";
    }
}
