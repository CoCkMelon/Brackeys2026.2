using UnityEngine;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Audio;

public class TemplateSmokeTest : MonoBehaviour
{
    private IInputService _input;
    private IAudioService _audio;

    private void Start()
    {
        _input = ServiceContainer.Get<IInputService>();
        _audio = ServiceContainer.Get<IAudioService>();

        _input.OnPause += () =>
        {
            Debug.Log("Pause pressed!");
            _audio.PlaySFX("sfx_click");
        };
    }

    private void Update()
    {
        var move = _input.Move;
        if (move.sqrMagnitude > 0.01f)
            Debug.Log($"Move: {move}");
    }
}
