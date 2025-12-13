namespace ZXTemplate.Audio
{
    public interface IAudioService
    {
        void PlayBGM(string id);
        void StopBGM();
        void PlaySFX(string id);
        void SetMixerVolume(string exposedParam, float volume01);
    }
}
