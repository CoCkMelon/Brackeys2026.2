using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZXTemplate.Core;
using ZXTemplate.Input;
using ZXTemplate.Settings;
using ZXTemplate.UI;

/// <summary>
/// Settings window with Apply/Cancel workflow and multiple tabs.
/// 
/// Key behaviors:
/// - Snapshot on open (ExportJsonSnapshot) so Cancel can restore everything.
/// - Apply writes settings to disk (Save) and updates the snapshot baseline.
/// - Audio/Video changes are applied immediately for preview (ApplyAll), but not saved until Apply.
/// - Video has a "safe revert" confirm dialog to prevent unusable resolutions/fullscreen settings.
/// - Uses InputModeService token to force UI input while settings is open.
/// - Optionally pauses the game while settings is open (pauseGameOnOpen).
/// </summary>
public class SettingsWindow : UIWindow
{
    public enum SettingsTab
    {
        Audio,
        Video,
        Controls
    }

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
    [Tooltip("If true, timeScale/pause state will be acquired while settings is open.")]
    [SerializeField] private bool pauseGameOnOpen = false;

    private ISettingsService _settings;
    private IInputModeService _inputMode;
    private IPauseService _pause;
    private IConfirmService _confirm;
    private IToastService _toast;

    // Full settings snapshot (used by Cancel / Back).
    private string _snapshotJson;

    // Video-only snapshot used by "safe revert".
    private string _confirmedVideoJson;
    private bool _videoConfirmOpen;

    // Tokens for temporary state changes while the window is open.
    private object _inputToken;
    private object _pauseToken;

    // Resolution dropdown cache.
    private readonly List<(int w, int h)> _resOptions = new();

    // Prevent UI callbacks from writing back while we are syncing UI from data.
    private bool _ignoreEvents;

    public override void OnPushed()
    {
        ResolveServices();
        AcquireTokens();

        // Save a baseline snapshot so Cancel can revert everything.
        _snapshotJson = _settings.ExportJsonSnapshot();

        // Video safe revert baseline starts from current video settings.
        _confirmedVideoJson = JsonUtility.ToJson(_settings.Data.video);
        _videoConfirmOpen = false;

        BuildDropdowns();
        BindUIEvents();

        // Initialize UI from current settings (no callbacks should run here).
        SyncUIFromSettings();

        // Default tab
        ShowTab(SettingsTab.Audio);
    }

    public override void OnPopped()
    {
        UnbindUIEvents();
        ReleaseTokens();
    }

    // -----------------------------
    // Setup / teardown helpers
    // -----------------------------
    private void ResolveServices()
    {
        _settings = ServiceContainer.Get<ISettingsService>();
        _inputMode = ServiceContainer.Get<IInputModeService>();
        _pause = ServiceContainer.Get<IPauseService>();
        _confirm = ServiceContainer.Get<IConfirmService>();
        _toast = ServiceContainer.Get<IToastService>();
    }

    private void AcquireTokens()
    {
        // Force UI input while settings is open.
        _inputToken = _inputMode.Acquire(InputMode.UI, "SettingsWindow");

        // Optionally pause gameplay while settings is open.
        if (pauseGameOnOpen)
            _pauseToken = _pause.Acquire("SettingsWindow");
    }

    private void ReleaseTokens()
    {
        if (_pauseToken != null) { _pause.Release(_pauseToken); _pauseToken = null; }
        if (_inputToken != null) { _inputMode.Release(_inputToken); _inputToken = null; }
    }

    private void BuildDropdowns()
    {
        BuildResolutionOptions();
        BuildQualityOptions();
    }

    private void BindUIEvents()
    {
        // Tabs
        tabAudioButton.onClick.AddListener(() => ShowTab(SettingsTab.Audio));
        tabVideoButton.onClick.AddListener(() => ShowTab(SettingsTab.Video));
        tabControlsButton.onClick.AddListener(() => ShowTab(SettingsTab.Controls));

        // Common
        backButton.onClick.AddListener(Back);
        applyButton.onClick.AddListener(Apply);
        cancelButton.onClick.AddListener(Cancel);

        // Audio
        masterSlider.onValueChanged.AddListener(OnMasterChanged);
        bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);

        masterMuteToggle.onValueChanged.AddListener(OnMasterMuteChanged);
        bgmMuteToggle.onValueChanged.AddListener(OnBgmMuteChanged);
        sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteChanged);

        // Video
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void UnbindUIEvents()
    {
        // Using RemoveAllListeners on tab buttons is OK here because these buttons
        // are owned by this window instance.
        tabAudioButton.onClick.RemoveAllListeners();
        tabVideoButton.onClick.RemoveAllListeners();
        tabControlsButton.onClick.RemoveAllListeners();

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
    }

    // -----------------------------
    // Tabs
    // -----------------------------
    private void ShowTab(SettingsTab tab)
    {
        // Simple tab switch by toggling panels.
        audioPanel.SetActive(tab == SettingsTab.Audio);
        videoPanel.SetActive(tab == SettingsTab.Video);
        controlsPanel.SetActive(tab == SettingsTab.Controls);
    }

    // -----------------------------
    // UI sync
    // -----------------------------
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

        // Resolution dropdown: pick best match
        int resIndex = FindResolutionIndex(v.width, v.height);
        if (resIndex < 0) resIndex = 0;
        resolutionDropdown.SetValueWithoutNotify(resIndex);

        // Quality dropdown
        int q = Mathf.Clamp(v.qualityIndex, 0, Mathf.Max(0, qualityDropdown.options.Count - 1));
        qualityDropdown.SetValueWithoutNotify(q);

        _ignoreEvents = false;
    }

    // -----------------------------
    // Audio handlers (preview apply)
    // -----------------------------
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

    // -----------------------------
    // Video handlers (preview apply + safe revert)
    // -----------------------------
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

    /// <summary>
    /// Opens a "Keep changes?" dialog once per preview session.
    /// If user does not confirm within timeout, we revert to the last confirmed video state.
    /// </summary>
    private void BeginVideoPreview()
    {
        // If the confirm dialog is already open, don't spawn another one.
        // Continued adjustments will still be preview-applied immediately.
        if (_videoConfirmOpen) return;

        _videoConfirmOpen = true;

        _confirm.Show(
            title: "Display Settings",
            message: "Keep changes?",
            confirmText: "Keep",
            cancelText: "Revert",
            onConfirm: () =>
            {
                // User confirms the current video settings as "safe".
                _confirmedVideoJson = JsonUtility.ToJson(_settings.Data.video);
                _videoConfirmOpen = false;
                _toast.Show("Display changed", 2f);
            },
            onCancel: () =>
            {
                // User cancels OR timeout happens -> revert to last confirmed video settings.
                ApplyConfirmedVideoSnapshot();
                _videoConfirmOpen = false;
                _toast.Show("Display reverted", 2f);
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

        // Only revert video (audio/controls remain as-is).
        _settings.Data.video = v;

        // Apply and refresh UI to reflect the reverted state.
        _settings.ApplyAll();
        SyncUIFromSettings();
    }

    // -----------------------------
    // Dropdown building
    // -----------------------------
    private void BuildResolutionOptions()
    {
        _resOptions.Clear();
        resolutionDropdown.ClearOptions();

        // Deduplicate by width x height.
        var set = new HashSet<(int, int)>();
        var list = new List<(int w, int h)>();

        foreach (var r in Screen.resolutions)
        {
            var key = (r.width, r.height);
            if (set.Add(key))
                list.Add(key);
        }

        // Fallback list for platforms/editors where resolutions are unavailable.
        if (list.Count == 0)
        {
            list.Add((1920, 1080));
            list.Add((1600, 900));
            list.Add((1280, 720));
        }

        // Sort by pixel count (small -> large).
        list.Sort((a, b) => (a.w * a.h).CompareTo(b.w * b.h));

        _resOptions.AddRange(list);

        var options = new List<string>(_resOptions.Count);
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

    // -----------------------------
    // Common actions
    // -----------------------------
    private void Back()
    {
        // Back behaves like Cancel, then closes the window.
        Cancel();
        ServiceContainer.Get<IUIService>().Pop();
    }

    private void Apply()
    {
        // Persist current settings to disk.
        // Note: During preview, MarkDirty() has already been called by UI handlers.
        _settings.MarkDirty();
        _settings.Save();

        // Update snapshot baseline so Cancel won't revert applied changes.
        _snapshotJson = _settings.ExportJsonSnapshot();

        _toast.Show("Settings applied", 1.5f);
    }

    private void Cancel()
    {
        // Restore the snapshot captured when the window was opened (no save).
        _settings.ImportJsonSnapshot(_snapshotJson, false);
        SyncUIFromSettings();
    }
}
