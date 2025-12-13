using System.IO;
using UnityEngine;

namespace ZXTemplate.Save
{
    public class JsonSaveService : ISaveService
    {
        private string PathOf(string key) =>
            System.IO.Path.Combine(Application.persistentDataPath, $"{key}.json");

        public void Save<T>(string key, T data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathOf(key), json);
        }

        public bool TryLoad<T>(string key, out T data)
        {
            var path = PathOf(key);
            if (!File.Exists(path))
            {
                data = default;
                return false;
            }

            var json = File.ReadAllText(path);
            data = JsonUtility.FromJson<T>(json);
            return true;
        }
    }
}
