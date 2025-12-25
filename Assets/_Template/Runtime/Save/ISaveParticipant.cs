namespace ZXTemplate.Save
{
    /// <summary>
    /// Implemented by systems that want to participate in SaveManager.SaveAll().
    ///
    /// Example participants:
    /// - SettingsService (save user preferences)
    /// - ProgressService (save gameplay progress)
    ///
    /// Rule:
    /// - Save() should be safe to call multiple times and should handle "not dirty" cases.
    /// </summary>
    public interface ISaveParticipant
    {
        /// <summary>
        /// Persists the system's current state (typically to ISaveService).
        /// </summary>
        void Save();
    }
}
