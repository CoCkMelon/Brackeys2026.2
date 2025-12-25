namespace ZXTemplate.Save
{
    /// <summary>
    /// Coordinates saving across multiple systems (participants).
    ///
    /// Typical usage:
    /// - Bootstrapper creates SaveManager and registers participants (Settings, Progress, etc.).
    /// - A runner (SaveManagerRunner) triggers SaveAll() on pause/quit.
    /// </summary>
    public interface ISaveManager
    {
        /// <summary>Registers a participant (no-op if already registered).</summary>
        void Register(ISaveParticipant participant);

        /// <summary>Unregisters a participant (no-op if not registered).</summary>
        void Unregister(ISaveParticipant participant);

        /// <summary>Calls Save() on all registered participants.</summary>
        void SaveAll();
    }
}
