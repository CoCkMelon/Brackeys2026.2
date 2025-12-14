using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Save;
using ZXTemplate.UI;

public class SettingsWindow : UIWindow
{
    [Header("UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private Button closeButton;

    [Header("Behavior")]
    [Tooltip("在游戏内打开设置时一般需要暂停；MainMenu里可以不暂停")]
    [SerializeField] private bool pauseGameOnOpen = true;

    private ISaveService _save;
    private IInputService _input;
    private IPauseService _pause;

    private SettingsData _data;
    private object _pauseToken;

    public override void OnPushed()
    {
        _save = ServiceContainer.Get<ISaveService>();
        _input = ServiceContainer.Get<IInputService>();
        _pause = ServiceContainer.Get<IPauseService>();

        // 1) Load
        if (!_save.TryLoad(SettingsKeys.Audio, out _data) || _data == null)
            _data = new SettingsData();

        // 2) Apply once (确保启动时也一致)
        SettingsApplier.ApplyAudio(_data);

        // 3) Pause & Input
        if (pauseGameOnOpen)
            _pauseToken = _pause.Acquire("Settings");

        _input.EnableUI();

        // 4) Init sliders (避免触发事件先把 value 赋好再绑事件)
        masterSlider.SetValueWithoutNotify(_data.master);
        bgmSlider.SetValueWithoutNotify(_data.bgm);
        sfxSlider.SetValueWithoutNotify(_data.sfx);

        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        closeButton.onClick.AddListener(Close);
    }

    public override void OnPopped()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBgmChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        closeButton.onClick.RemoveListener(Close);

        // Save on close
        _save.Save(SettingsKeys.Audio, _data);

        // Resume & input back
        if (_pauseToken != null)
        {
            _pause.Release(_pauseToken);
            _pauseToken = null;
        }

        _input.EnableGameplay();
    }

    private void OnMasterChanged(float v)
    {
        _data.master = v;
        SettingsApplier.ApplyAudio(_data);
    }

    private void OnBgmChanged(float v)
    {
        _data.bgm = v;
        SettingsApplier.ApplyAudio(_data);
    }

    private void OnSfxChanged(float v)
    {
        _data.sfx = v;
        SettingsApplier.ApplyAudio(_data);
    }

    private void Close()
    {
        ServiceContainer.Get<IUIService>().Pop();
    }
}
