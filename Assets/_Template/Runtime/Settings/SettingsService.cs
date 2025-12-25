using System;
using UnityEngine;
using ZXTemplate.Save;

namespace ZXTemplate.Settings
{
    /// <summary>
    /// Owns the persistent SettingsData and applies it to runtime systems.
    ///
    /// Responsibilities:
    /// 1) Load / create SettingsData via ISaveService
    /// 2) ApplyAll() to runtime (Audio mixer, Screen/Resolution, Input bindings)
    /// 3) Track "dirty" state and Save() only when explicitly requested (Apply button)
    ///
    /// Design notes:
    /// - UI may "preview" settings by calling ApplyAll() without saving.
    /// - Apply/Cancel in Settings UI is implemented via JSON snapshots
    ///   (ExportJsonSnapshot / ImportJsonSnapshot).
    /// - Clamp() is always called before applying or saving to ensure data is valid.
    /// </summary>
    public class SettingsService : ISettingsService, ISaveParticipant
    {
        /// <summary>Current settings data in memory.</summary>
        public SettingsData Data { get; private set; }

        /// <summary>Raised when settings are modified or reloaded.</summary>
        public event Action OnChanged;

        private readonly ISaveService _save;
        private bool _dirty;

        public SettingsService(ISaveService save)
        {
            _save = save;
            LoadOrCreate();

            // Apply once so runtime starts with correct volume/resolution/bindings.
            ApplyAll();

            _dirty = false;
        }

        /// <summary>
        /// Loads existing settings from disk; if missing/invalid, creates defaults.
        /// </summary>
        private void LoadOrCreate()
        {
            if (!_save.TryLoad(SettingsKeys.Main, out SettingsData data) || data == null || !data.initialized)
            {
                // First run (or corrupted data): create default settings and persist immediately.
                Data = new SettingsData();
                _save.Save(SettingsKeys.Main, Data);
                return;
            }

            // Migration hook: keep old saves compatible when SettingsData evolves.
            if (data.version != SettingsData.CurrentVersion)
            {
                // TODO: future migrations
                data.version = SettingsData.CurrentVersion;
            }

            data.Clamp();
            Data = data;
        }

        /// <summary>
        /// Marks settings as modified.
        /// Caller decides when to Save() (typically the Settings UI "Apply" button).
        /// </summary>
        public void MarkDirty()
        {
            _dirty = true;
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Applies current settings to runtime systems.
        /// This does NOT save to disk.
        /// Useful for "preview" while the settings UI is open.
        /// </summary>
        public void ApplyAll()
        {
            Data.Clamp();

            SettingsApplierAudio.Apply(Data.audio);
            SettingsApplierVideo.Apply(Data.video);
            SettingsApplierControls.Apply(Data.controls);
        }

        /// <summary>
        /// Saves current settings to disk only if marked dirty.
        /// </summary>
        public void Save()
        {
            if (!_dirty) return;

            Data.Clamp();
            _save.Save(SettingsKeys.Main, Data);

            _dirty = false;
        }

        /// <summary>
        /// Resets settings to defaults and applies immediately.
        /// This also writes defaults to disk right away.
        /// </summary>
        public void ResetToDefault()
        {
            Data = new SettingsData();
            _save.Save(SettingsKeys.Main, Data);
            ApplyAll();
            OnChanged?.Invoke();
        }

        /// <summary>
        /// Exports a deep-copy snapshot of SettingsData as JSON.
        /// Used by Settings UI to implement Apply/Cancel behavior.
        /// </summary>
        public string ExportJsonSnapshot()
        {
            return JsonUtility.ToJson(Data);
        }

        /// <summary>
        /// Imports a SettingsData snapshot from JSON and applies immediately.
        /// markDirty = true means caller intends to save later; false means "preview/cancel".
        /// </summary>
        public void ImportJsonSnapshot(string json, bool markDirty)
        {
            if (string.IsNullOrEmpty(json)) return;

            var loaded = JsonUtility.FromJson<SettingsData>(json);
            if (loaded == null) return;

            loaded.Clamp();
            Data = loaded;

            _dirty = markDirty;

            // Apply right away so runtime matches the restored snapshot.
            ApplyAll();

            OnChanged?.Invoke();
        }
    }
}
