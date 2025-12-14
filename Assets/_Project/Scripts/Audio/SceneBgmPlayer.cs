using UnityEngine;
using ZXTemplate.Audio;
using ZXTemplate.Core;

public class SceneBgmPlayer : MonoBehaviour
{
    [SerializeField] private string bgmId = "bgm_menu";

    private void Start()
    {
        ServiceContainer.Get<IAudioService>().PlayBGM(bgmId);
    }
}
