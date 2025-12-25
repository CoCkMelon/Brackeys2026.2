using System;

namespace ZXTemplate.Settings
{
    public interface ISettingsService
    {
        SettingsData Data { get; }
        event Action OnChanged;

        void MarkDirty();     // slider
        void ApplyAll();
        void Save();
        void ResetToDefault();

        string ExportJsonSnapshot();
        void ImportJsonSnapshot(string json, bool markDirty);
    }
}
