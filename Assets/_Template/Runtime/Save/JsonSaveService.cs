using System;
using System.IO;
using UnityEngine;

namespace ZXTemplate.Save
{
    /// <summary>
    /// Simple JSON persistence using Unity JsonUtility.
    ///
    /// Storage location:
    /// - Application.persistentDataPath (per-platform safe writable folder)
    ///
    /// Key format:
    /// - Each key maps to a single file: "{key}.json"
    ///
    /// Notes:
    /// - JsonUtility is fast and simple but has limitations (no Dictionary by default, etc.).
    /// - This service is intentionally minimal for course projects / templates.
    /// </summary>
    public class JsonSaveService : ISaveService
    {
        private string PathOf(string key)
            => System.IO.Path.Combine(Application.persistentDataPath, $"{key}.json");

        /// <summary>
        /// Saves an object to disk as pretty-printed JSON.
        /// Overwrites existing file.
        /// </summary>
        public void Save<T>(string key, T data)
        {
            var path = PathOf(key);

            try
            {
                var json = JsonUtility.ToJson(data, true);

                // Ensure folder exists (should already exist, but safe on some platforms).
                Directory.CreateDirectory(Path.GetDirectoryName(path));

                // Optional: atomic save (write temp then replace) to reduce corruption risk.
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                File.Copy(tmp, path, true);
                File.Delete(tmp);
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonSaveService] Save failed. key='{key}' path='{path}'\n{e}");
            }
        }

        /// <summary>
        /// Attempts to load and deserialize JSON from disk.
        /// Returns false if file does not exist or data cannot be parsed.
        /// </summary>
        public bool TryLoad<T>(string key, out T data)
        {
            var path = PathOf(key);

            try
            {
                if (!File.Exists(path))
                {
                    data = default;
                    return false;
                }

                var json = File.ReadAllText(path);
                data = JsonUtility.FromJson<T>(json);

                // JsonUtility.FromJson may return null for reference types when json is invalid.
                if (data == null)
                    return false;

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JsonSaveService] Load failed. key='{key}' path='{path}'\n{e}");
                data = default;
                return false;
            }
        }
    }
}
