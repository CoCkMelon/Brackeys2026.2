namespace ZXTemplate.Save
{
    /// <summary>
    /// Low-level persistence API (key-value).
    ///
    /// Typical usage:
    /// - Save("settings", SettingsData)
    /// - TryLoad("progress", out ProgressData)
    ///
    /// Notes:
    /// - This interface intentionally stays minimal for template/course projects.
    /// - Implementations may use JSON, binary, PlayerPrefs, cloud, etc.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Saves data under a key. Overwrites existing data for that key.
        /// </summary>
        void Save<T>(string key, T data);

        /// <summary>
        /// Tries to load data by key. Returns false if not found or deserialization fails.
        /// </summary>
        bool TryLoad<T>(string key, out T data);
    }
}
