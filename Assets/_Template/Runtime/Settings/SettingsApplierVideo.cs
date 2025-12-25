using UnityEngine;

namespace ZXTemplate.Settings
{
    /// <summary>
    /// Applies VideoSettings to Unity runtime (Quality + Resolution + Fullscreen mode).
    ///
    /// Notes:
    /// - We store width/height in settings (not dropdown index) to keep it stable across machines.
    /// - For course projects, FullScreenWindow + Windowed usually covers most needs.
    /// </summary>
    public static class SettingsApplierVideo
    {
        public static void Apply(VideoSettings v)
        {
            v.Clamp();

            // Quality level (safe even if names array is empty in rare cases).
            if (QualitySettings.names.Length > 0)
                QualitySettings.SetQualityLevel(v.qualityIndex, applyExpensiveChanges: true);

            // Fullscreen mode (simple but good enough for course projects).
            var mode = v.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

            // Resolution
            Screen.SetResolution(v.width, v.height, mode);
        }
    }
}
