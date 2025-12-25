using ZXTemplate.Audio;
using ZXTemplate.Core;

namespace ZXTemplate.Settings
{
    /// <summary>
    /// Applies AudioSettings to the runtime AudioMixer through IAudioService.
    ///
    /// Convention:
    /// - Mixer exposed parameters must exist in your AudioMixer:
    ///   "Master", "BGM", "SFX"
    ///
    /// Mute behavior:
    /// - We apply 0 volume when muted (AudioService converts 0 -> -80 dB).
    /// - This is predictable and avoids disabling AudioSources.
    /// </summary>
    public static class SettingsApplierAudio
    {
        // AudioMixer exposed parameter names (must match exactly).
        private const string MasterParam = "Master";
        private const string BgmParam = "BGM";
        private const string SfxParam = "SFX";

        public static void Apply(AudioSettings a)
        {
            var audio = ServiceContainer.Get<IAudioService>();

            // Apply mute by forcing volume to 0.
            var master = a.masterMuted ? 0f : a.master;
            var bgm = a.bgmMuted ? 0f : a.bgm;
            var sfx = a.sfxMuted ? 0f : a.sfx;

            audio.SetMixerVolume(MasterParam, master);
            audio.SetMixerVolume(BgmParam, bgm);
            audio.SetMixerVolume(SfxParam, sfx);
        }
    }
}
