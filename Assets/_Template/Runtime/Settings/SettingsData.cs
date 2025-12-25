using System;
using UnityEngine;

namespace ZXTemplate.Settings
{
    /// <summary>
    /// Root settings data persisted by SettingsService.
    ///
    /// Contains:
    /// - Audio settings (mixer volumes + mute flags)
    /// - Video settings (fullscreen / resolution / quality)
    /// - Controls settings (input binding overrides)
    ///
    /// Versioning:
    /// - Keep a version field for future migrations.
    /// - initialized is used to detect first-run / corrupted saves.
    /// </summary>
    [Serializable]
    public class SettingsData
    {
        public const int CurrentVersion = 1;

        public bool initialized = true;
        public int version = CurrentVersion;

        public AudioSettings audio = new();
        public VideoSettings video = new();
        public ControlsSettings controls = new();

        /// <summary>
        /// Clamps all sub-settings to valid ranges.
        /// Call before applying or saving.
        /// </summary>
        public void Clamp()
        {
            audio.Clamp();
            video.Clamp();
            // controls: no numeric clamp needed
        }
    }

    /// <summary>
    /// Audio settings in normalized form (0..1) plus mute flags.
    /// Actual runtime volume is applied via AudioMixer exposed parameters (dB conversion).
    /// </summary>
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

    /// <summary>
    /// Video/display settings.
    ///
    /// Notes:
    /// - We store width/height of the currently selected resolution (not an index),
    ///   so the setting stays stable even if the available resolutions list changes
    ///   between machines/monitors.
    /// - qualityIndex aligns with QualitySettings.names.
    /// </summary>
    [Serializable]
    public class VideoSettings
    {
        public bool fullscreen = true;

        // Store selected resolution as width/height (not dropdown index).
        public int width = 1920;
        public int height = 1080;

        // Aligns with QualitySettings.names
        public int qualityIndex = 0;

        public void Clamp()
        {
            width = Mathf.Max(640, width);
            height = Mathf.Max(360, height);

            var maxQ = Mathf.Max(0, QualitySettings.names.Length - 1);
            qualityIndex = Mathf.Clamp(qualityIndex, 0, maxQ);
        }
    }

    /// <summary>
    /// Controls settings.
    ///
    /// bindingOverridesJson is the JSON string produced by:
    /// InputActionAsset.SaveBindingOverridesAsJson()
    /// and restored by:
    /// InputActionAsset.LoadBindingOverridesFromJson(...)
    /// </summary>
    [Serializable]
    public class ControlsSettings
    {
        public string bindingOverridesJson = "";
    }
}
