namespace ZXTemplate.Audio
{
    /// <summary>
    /// High-level audio API for gameplay code.
    ///
    /// Responsibilities:
    /// - Play background music (BGM) by string id
    /// - Play sound effects (SFX) by string id
    /// - Control AudioMixer exposed parameters using normalized (0..1) volume
    ///
    /// Notes:
    /// - The underlying implementation may use AudioSource, Addressables, pooling, etc.
    /// - This interface stays small for course/template projects.
    /// </summary>
    public interface IAudioService
    {
        void PlayBGM(string id);
        void StopBGM();

        void PlaySFX(string id);

        /// <summary>
        /// Sets an AudioMixer exposed parameter using normalized volume (0..1).
        /// The implementation converts to decibels (dB) internally.
        /// </summary>
        void SetMixerVolume(string exposedParam, float volume01);

        /// <summary>
        /// Reads an AudioMixer exposed parameter value (dB).
        /// Returns false if the parameter does not exist.
        /// </summary>
        bool TryGetMixerDb(string exposedParam, out float db);
    }
}
