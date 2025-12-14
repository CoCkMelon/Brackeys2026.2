using UnityEngine;
using UnityEngine.Audio;

namespace ZXTemplate.Audio
{
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

            _root = new GameObject("@Audio");
            Object.DontDestroyOnLoad(_root);

            _bgm = _root.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;
            _bgm.outputAudioMixerGroup = bgmGroup;

            _sfx = _root.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
            _sfx.outputAudioMixerGroup = sfxGroup;
        }

        public void PlayBGM(string id)
        {
            var clip = _library.Find(id);
            if (clip == null) return;

            if (_bgm.clip == clip && _bgm.isPlaying) return;

            _bgm.clip = clip;
            _bgm.Play();
        }

        public void StopBGM() => _bgm.Stop();

        public void PlaySFX(string id)
        {
            var clip = _library.Find(id);
            if (clip == null) return;
            _sfx.PlayOneShot(clip);
        }

        public void SetMixerVolume(string exposedParam, float volume01)
        {
            var db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(volume01));
            _mixer.SetFloat(exposedParam, db);
        }
    }
}
