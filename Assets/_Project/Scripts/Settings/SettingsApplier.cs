using ZXTemplate.Core;
using ZXTemplate.Audio;

public static class SettingsApplier
{
    // 这里的参数名要和 AudioMixer 暴露的 Exposed Parameter 完全一致
    public const string MasterParam = "Master";
    public const string BgmParam = "BGM";
    public const string SfxParam = "SFX";

    public static void ApplyAudio(SettingsData data)
    {
        var audio = ServiceContainer.Get<IAudioService>();
        audio.SetMixerVolume(MasterParam, data.master);
        audio.SetMixerVolume(BgmParam, data.bgm);
        audio.SetMixerVolume(SfxParam, data.sfx);
    }
}
