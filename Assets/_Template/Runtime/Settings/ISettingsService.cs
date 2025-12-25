using System;

namespace ZXTemplate.Settings
{
    /// <summary>
    /// High-level settings API (user preferences).
    ///
    /// Design goals:
    /// - Support "preview" changes while Settings UI is open (ApplyAll without saving).
    /// - Support Apply/Cancel workflow via JSON snapshots.
    /// - Persist to disk only when explicitly requested (Save / Apply button).
    ///
    /// Typical Settings UI flow:
    /// 1) snapshot = ExportJsonSnapshot()
    /// 2) user changes sliders/toggles -> update Data -> MarkDirty() -> ApplyAll() (preview)
    /// 3) user clicks Apply -> Save()
    /// 4) user clicks Cancel -> ImportJsonSnapshot(snapshot, markDirty:false)
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>Current settings data in memory.</summary>
        SettingsData Data { get; }

        /// <summary>Raised when settings are changed or restored.</summary>
        event Action OnChanged;

        /// <summary>
        /// Marks settings as modified (dirty). Does not save by itself.
        /// Settings UI should call this when user changes a value.
        /// </summary>
        void MarkDirty();

        /// <summary>
        /// Applies current Data to runtime systems (Audio/Video/Controls).
        /// Does NOT write to disk.
        /// </summary>
        void ApplyAll();

        /// <summary>
        /// Persists current Data to disk (usually called by Apply button).
        /// Implementations may ignore if nothing changed.
        /// </summary>
        void Save();

        /// <summary>
        /// Resets settings to defaults and applies immediately.
        /// Typically also writes defaults to disk.
        /// </summary>
        void ResetToDefault();

        /// <summary>
        /// Exports a deep-copy snapshot of SettingsData as JSON.
        /// Used to implement Apply/Cancel.
        /// </summary>
        string ExportJsonSnapshot();

        /// <summary>
        /// Imports a snapshot from JSON, applies immediately, and optionally marks as dirty.
        /// markDirty=false is typically used by Cancel to revert changes without saving.
        /// </summary>
        void ImportJsonSnapshot(string json, bool markDirty);
    }
}
