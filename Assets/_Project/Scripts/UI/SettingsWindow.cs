using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;
using ZXTemplate.UI;

public enum SettingsTab
{
    Audio,
    Video,
    Controls
}

public class SettingsWindow : UIWindow
{
    [Header("Tabs")]
    [SerializeField] private Button tabAudioButton;
    [SerializeField] private Button tabVideoButton;
    [SerializeField] private Button tabControlsButton;
    [SerializeField] private GameObject audioPanel;
    [SerializeField] private GameObject videoPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Common")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;

    [Header("Audio UI")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Toggle masterMuteToggle;

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Toggle bgmMuteToggle;

    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("Video UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnOpen = false;

    private ISettingsService _settings;
    private IInputModeService _inputMode;
    private IPauseService _pause;
    private string _snapshotJson;
    private IConfirmService _confirm;
    private string _confirmedVideoJson;
    private bool _videoConfirmOpen;

    private object _inputToken;
    private object _pauseToken;

    private readonly List<(int w, int h)> _resOptions = new();
    private bool _ignoreEvents;

    public override void OnPushed()
    {
        _settings = ServiceContainer.Get<ISettingsService>();
        _inputMode = ServiceContainer.Get<IInputModeService>();
        _pause = ServiceContainer.Get<IPauseService>();

        _confirm = ServiceContainer.Get<ZXTemplate.UI.IConfirmService>();
        _confirmedVideoJson = JsonUtility.ToJson(_settings.Data.video);
        _videoConfirmOpen = false;

        _inputToken = _inputMode.Acquire(InputMode.UI, "SettingsWindow");
        if (pauseGameOnOpen)
            _pauseToken = _pause.Acquire("SettingsWindow");

        // save snapshot
        _snapshotJson = _settings.ExportJsonSnapshot();

        // Tabs
        tabAudioButton.onClick.AddListener(() => ShowTab(SettingsTab.Audio));
        tabVideoButton.onClick.AddListener(() => ShowTab(SettingsTab.Video));
        tabControlsButton.onClick.AddListener(() => ShowTab(SettingsTab.Controls));

        // Common
        backButton.onClick.AddListener(Back);
        applyButton.onClick.AddListener(Apply);
        cancelButton.onClick.AddListener(Cancel);

        // Prepare Video dropdowns
        BuildResolutionOptions();
        BuildQualityOptions();

        // Bind UI events
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        masterMuteToggle.onValueChanged.AddListener(OnMasterMuteChanged);
        bgmMuteToggle.onValueChanged.AddListener(OnBgmMuteChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteChanged);

        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        // Init UI from data
        SyncUIFromSettings();

        // Default tab
        ShowTab(SettingsTab.Audio);
    }

    public override void OnPopped()
    {
        tabAudioButton.onClick.RemoveAllListeners();
        tabVideoButton.onClick.RemoveAllListeners();
        backButton.onClick.RemoveListener(Back);
        applyButton.onClick.RemoveListener(Apply);
        cancelButton.onClick.RemoveListener(Cancel);

        masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
        bgmSlider.onValueChanged.RemoveListener(OnBgmChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

        masterMuteToggle.onValueChanged.RemoveListener(OnMasterMuteChanged);
        bgmMuteToggle.onValueChanged.RemoveListener(OnBgmMuteChanged);
        sfxMuteToggle.onValueChanged.RemoveListener(OnSfxMuteChanged);

        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);

        if (_pauseToken != null) { _pause.Release(_pauseToken); _pauseToken = null; }
        if (_inputToken != null) { _inputMode.Release(_inputToken); _inputToken = null; }
    }

    private void ShowTab(SettingsTab tab)
    {
        switch(tab)
        {
            case SettingsTab.Audio:
                audioPanel.SetActive(true);
                videoPanel.SetActive(false);
                controlsPanel.SetActive(false);
                break;
            case SettingsTab.Video:
                audioPanel.SetActive(false);
                videoPanel.SetActive(true);
                controlsPanel.SetActive(false);
                break;
            case SettingsTab.Controls:
                audioPanel.SetActive(false);
                videoPanel.SetActive(false);
                controlsPanel.SetActive(true);
                break;
        }
    }

    private void SyncUIFromSettings()
    {
        _ignoreEvents = true;

        var a = _settings.Data.audio;
        masterSlider.SetValueWithoutNotify(a.master);
        bgmSlider.SetValueWithoutNotify(a.bgm);
        sfxSlider.SetValueWithoutNotify(a.sfx);

        masterMuteToggle.SetIsOnWithoutNotify(a.masterMuted);
        bgmMuteToggle.SetIsOnWithoutNotify(a.bgmMuted);
        sfxMuteToggle.SetIsOnWithoutNotify(a.sfxMuted);

        var v = _settings.Data.video;
        fullscreenToggle.SetIsOnWithoutNotify(v.fullscreen);

        // resolution dropdown: find best match
        int resIndex = FindResolutionIndex(v.width, v.height);
        if (resIndex < 0) resIndex = 0;
        resolutionDropdown.SetValueWithoutNotify(resIndex);

        // quality dropdown
        int q = Mathf.Clamp(v.qualityIndex, 0, Mathf.Max(0, qualityDropdown.options.Count - 1));
        qualityDropdown.SetValueWithoutNotify(q);

        _ignoreEvents = false;
    }

    // -------- Audio handlers --------
    private void OnMasterChanged(float v)
    {
        if (_ignoreEvents) return;
        _settings.Data.audio.master = v;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnBgmChanged(float v)
    {
        if (_ignoreEvents) return;
        _settings.Data.audio.bgm = v;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnSfxChanged(float v)
    {
        if (_ignoreEvents) return;
        _settings.Data.audio.sfx = v;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnMasterMuteChanged(bool on)
    {
        if (_ignoreEvents) return;
        _settings.Data.audio.masterMuted = on;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnBgmMuteChanged(bool on)
    {
        if (_ignoreEvents) return;
        _settings.Data.audio.bgmMuted = on;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnSfxMuteChanged(bool on)
    {
        if (_ignoreEvents) return;
        _settings.Data.audio.sfxMuted = on;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    // -------- Video handlers --------
    private void OnResolutionChanged(int index)
    {
        if (_ignoreEvents) return;
        if (index < 0 || index >= _resOptions.Count) return;

        BeginVideoPreview();

        var (w, h) = _resOptions[index];
        _settings.Data.video.width = w;
        _settings.Data.video.height = h;

        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnFullscreenChanged(bool on)
    {
        if (_ignoreEvents) return;

        BeginVideoPreview();

        _settings.Data.video.fullscreen = on;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void OnQualityChanged(int index)
    {
        if (_ignoreEvents) return;

        BeginVideoPreview();

        _settings.Data.video.qualityIndex = index;
        _settings.MarkDirty();
        _settings.ApplyAll();
    }

    private void BeginVideoPreview()
    {
        // 已经弹过确认框，就不重复弹；用户继续改动也不会开多个
        if (_videoConfirmOpen) return;

        _videoConfirmOpen = true;

        _confirm.Show(
            title: "Display Settings",
            message: "Keep changes?",
            confirmText: "Keep",
            cancelText: "Revert",
            onConfirm: () =>
            {
                // Keep：更新“已确认”
                _confirmedVideoJson = JsonUtility.ToJson(_settings.Data.video);
                _videoConfirmOpen = false;
            },
            onCancel: () =>
            {
                // Revert（或超时）：回滚到已确认
                ApplyConfirmedVideoSnapshot();
                _videoConfirmOpen = false;
            },
            timeoutSeconds: 10f,
            timeoutAsCancel: true
        );
    }

    private void ApplyConfirmedVideoSnapshot()
    {
        if (string.IsNullOrEmpty(_confirmedVideoJson)) return;

        var v = JsonUtility.FromJson<ZXTemplate.Settings.VideoSettings>(_confirmedVideoJson);
        if (v == null) return;

        // 只回滚 video，不影响 audio/controls
        _settings.Data.video = v;

        // 应用并刷新 UI
        _settings.ApplyAll();
        SyncUIFromSettings(); // 你已有的方法，里面 _ignoreEvents 会保护
    }

    // -------- Dropdown building --------
    private void BuildResolutionOptions()
    {
        _resOptions.Clear();
        resolutionDropdown.ClearOptions();

        // 用系统支持分辨率去重（只保留 w×h）
        var set = new HashSet<(int, int)>();
        var list = new List<(int w, int h)>();

        foreach (var r in Screen.resolutions)
        {
            var key = (r.width, r.height);
            if (set.Add(key))
                list.Add(key);
        }

        // 如果拿不到（某些平台/编辑器情况），至少给几个常用
        if (list.Count == 0)
        {
            list.Add((1920, 1080));
            list.Add((1600, 900));
            list.Add((1280, 720));
        }

        // 排序：从小到大
        list.Sort((a, b) =>
        {
            int pa = a.w * a.h;
            int pb = b.w * b.h;
            return pa.CompareTo(pb);
        });

        _resOptions.AddRange(list);

        var options = new List<string>();
        for (int i = 0; i < _resOptions.Count; i++)
            options.Add($"{_resOptions[i].w} x {_resOptions[i].h}");

        resolutionDropdown.AddOptions(options);
    }

    private int FindResolutionIndex(int w, int h)
    {
        for (int i = 0; i < _resOptions.Count; i++)
            if (_resOptions[i].w == w && _resOptions[i].h == h) return i;
        return -1;
    }

    private void BuildQualityOptions()
    {
        qualityDropdown.ClearOptions();
        var names = QualitySettings.names;
        var options = new List<string>(names.Length);
        for (int i = 0; i < names.Length; i++)
            options.Add(names[i]);
        qualityDropdown.AddOptions(options);
    }

    private void Back()
    {
        Cancel();
        ServiceContainer.Get<IUIService>().Pop();
    }

    private void Apply()
    {
        _settings.MarkDirty();
        _settings.Save();
        
        _snapshotJson = _settings.ExportJsonSnapshot();

        ServiceContainer.Get<IToastService>().Show("Settings applied", 1.5f);
    }

    private void Cancel()
    {
        _settings.ImportJsonSnapshot(_snapshotJson, false);
        SyncUIFromSettings();
    }
}
