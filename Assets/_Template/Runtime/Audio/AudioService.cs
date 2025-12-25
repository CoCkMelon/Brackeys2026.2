using UnityEngine;
using UnityEngine.Audio;

namespace ZXTemplate.Audio
{
    /// <summary>
    /// Default audio implementation for the template.
    ///
    /// Creates a persistent audio root GameObject ("@Audio") with:
    /// - AudioListener
    /// - One looping AudioSource for BGM
    /// - One AudioSource for SFX (PlayOneShot)
    ///
    /// Notes:
    /// - Root is marked DontDestroyOnLoad so audio continues across scene transitions.
    /// - AudioSources use ignoreListenerPause=true so UI sounds can still play while paused
    ///   (pause menus/settings often set Time.timeScale=0, or you may toggle AudioListener.pause).
    /// - Volume is controlled via AudioMixer exposed parameters (see SetMixerVolume).
    /// - All scenes should NOT have AudioListener on cameras. AudioService owns the only listener.
    /// </summary>
    public class AudioService : IAudioService
    {
        private readonly AudioMixer _mixer;
        private readonly AudioLibrary _library;

        private readonly GameObject _root;
        private readonly AudioSource _bgm;
        private readonly AudioSource _sfx;

        public AudioService(AudioMixer mixer, AudioLibrary library, AudioMixerGroup bgmGroup, AudioMixerGroup sfxGroup)
        {
            _mixer = mixer;
            _library = library;

            // Persistent root
            _root = new GameObject("@Audio");
            Object.DontDestroyOnLoad(_root);

            // Template choice: keep only one AudioListener managed by this service.
            // (Avoid placing AudioListener on cameras in scenes to prevent duplicates.)
            _root.AddComponent<AudioListener>();

            // BGM source (looping)
            _bgm = _root.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;
            _bgm.outputAudioMixerGroup = bgmGroup;
            _bgm.ignoreListenerPause = true;

            // SFX source (one-shot)
            _sfx = _root.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.outputAudioMixerGroup = sfxGroup;
            _sfx.ignoreListenerPause = true;
        }

        /// <summary>
        /// Plays a BGM clip by id (from AudioLibrary). If already playing the same clip, do nothing.
        /// </summary>
        public void PlayBGM(string id)
        {
            var clip = _library.Find(id);
            if (clip == null) return;

            if (_bgm.clip == clip && _bgm.isPlaying) return;

            _bgm.clip = clip;
            _bgm.Play();
        }

        public void StopBGM() => _bgm.Stop();

        /// <summary>
        /// Plays an SFX clip by id using PlayOneShot.
        /// </summary>
        public void PlaySFX(string id)
        {
            var clip = _library.Find(id);
            if (clip == null) return;
            _sfx.PlayOneShot(clip);
        }

        /// <summary>
        /// Sets an AudioMixer exposed parameter using normalized volume [0..1].
        ///
        /// We convert to decibels (dB):
        /// - 1.0 ->  0 dB
        /// - 0.5 -> ~-6 dB
        /// - 0.1 -> -20 dB
        /// - 0.0 -> -80 dB (silence floor)
        ///
        /// Important:
        /// - The exposed parameter must be created in AudioMixer (usually on group attenuation volume).
        /// </summary>
        public void SetMixerVolume(string exposedParam, float volume01)
        {
            volume01 = Mathf.Clamp01(volume01);

            // Convert linear volume to dB. Use a floor to avoid -Infinity.
            float db = (volume01 <= 0.0001f) ? -80f : Mathf.Log10(volume01) * 20f;

            if (!_mixer.SetFloat(exposedParam, db))
                Debug.LogWarning($"AudioMixer exposed param not found: {exposedParam}");
        }

        /// <summary>
        /// Tries to get the current dB value of an exposed AudioMixer parameter.
        /// </summary>
        public bool TryGetMixerDb(string exposedParam, out float db)
        {
            return _mixer.GetFloat(exposedParam, out db);
        }
    }
}
