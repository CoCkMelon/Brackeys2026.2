using System.Collections.Generic;

namespace ZXTemplate.Save
{
    /// <summary>
    /// Coordinates saving across multiple systems.
    ///
    /// Concept:
    /// - Individual systems (SettingsService, ProgressService, etc.) implement ISaveParticipant.
    /// - SaveManager keeps a list of participants and calls Save() on each when requested.
    ///
    /// Why this exists:
    /// - Central place to trigger saving (e.g. on "Apply", on scene change, on app quit).
    /// - Avoids every system trying to save at the same time or duplicating save triggers.
    ///
    /// Notes:
    /// - This class intentionally does NOT decide *when* to save.
    ///   A separate runner (SaveManagerRunner) can hook Unity lifecycle events if needed.
    /// </summary>
    public class SaveManager : ISaveManager
    {
        private readonly List<ISaveParticipant> _participants = new();

        /// <summary>
        /// Registers a participant. Duplicate registrations are ignored.
        /// </summary>
        public void Register(ISaveParticipant participant)
        {
            if (participant == null) return;
            if (_participants.Contains(participant)) return;
            _participants.Add(participant);
        }

        /// <summary>
        /// Unregisters a participant. No-op if not registered.
        /// </summary>
        public void Unregister(ISaveParticipant participant)
        {
            if (participant == null) return;
            _participants.Remove(participant);
        }

        /// <summary>
        /// Calls Save() on all registered participants.
        /// </summary>
        public void SaveAll()
        {
            for (int i = 0; i < _participants.Count; i++)
                _participants[i].Save();
        }
    }
}
