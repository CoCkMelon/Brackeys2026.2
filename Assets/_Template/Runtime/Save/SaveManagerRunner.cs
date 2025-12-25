using UnityEngine;

namespace ZXTemplate.Save
{
    /// <summary>
    /// Unity lifecycle hook that triggers SaveManager.SaveAll().
    ///
    /// Why we need a runner:
    /// - SaveManager itself is a pure C# class (no MonoBehaviour callbacks).
    /// - This runner bridges Unity events (pause/quit/destroy) to SaveManager.
    ///
    /// When it saves:
    /// - OnApplicationPause(true): app is going to background (mobile / window focus changes).
    /// - OnApplicationQuit(): app is quitting.
    /// - OnDestroy(): last safety net if the runner gets destroyed unexpectedly.
    ///
    /// Notes:
    /// - SaveAll() should be fast and participant Save() should handle "not dirty" cases.
    /// - OnDestroy saving can be redundant, but it's safe for small template projects.
    /// </summary>
    public class SaveManagerRunner : MonoBehaviour
    {
        private ISaveManager _manager;

        /// <summary>
        /// Called by Bootstrapper after creating SaveManager.
        /// </summary>
        public void Initialize(ISaveManager manager)
        {
            _manager = manager;
        }

        private void OnApplicationPause(bool paused)
        {
            // Save when app is being paused / backgrounded.
            if (paused) _manager?.SaveAll();
        }

        private void OnApplicationQuit()
        {
            // Save on quit.
            _manager?.SaveAll();
        }

        private void OnDestroy()
        {
            // Final safety net (e.g. domain reload / scene reset / unexpected destruction).
            _manager?.SaveAll();
        }
    }
}
