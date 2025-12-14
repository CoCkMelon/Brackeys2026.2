using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Audio;
using ZXTemplate.Core;

[RequireComponent(typeof(Button))]
public class UIButtonClickSfx : MonoBehaviour
{
    [SerializeField] private string sfxId = "sfx_click";

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(Play);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(Play);
    }

    private void Play()
    {
        ServiceContainer.Get<IAudioService>().PlaySFX(sfxId);
    }
}
