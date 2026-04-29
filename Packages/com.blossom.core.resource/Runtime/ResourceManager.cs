namespace Blossom.Core.Resource {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Object = UnityEngine.Object;
    
    internal static class ResourceManager {

        private static readonly Dictionary<Type, Dictionary<string, Object>> _cache = new();

        internal static void Load<T>(string path, ResourceKeyPolicy keyPolicy, Func<T, string> keySelector)
            where T : Object {
            if (string.IsNullOrEmpty(path)) {
                Debug.LogError($"Path({path}) is null or empty. (Type: {typeof(T).Name})");
                return;
            }

            T[] loaded = Resources.LoadAll<T>(path);
            Dictionary<string, Object> dictionary = new();
            if (loaded != null) {
                foreach (T obj in loaded) {
                    if (obj == null) continue;
                    string key = GetKey(obj, keyPolicy, keySelector);
                    if (string.IsNullOrEmpty(key)) continue;
                    dictionary[key] = obj;
                }
            }

            _cache[typeof(T)] = dictionary;
        }

        internal static bool Contains<T>(string key) where T : Object {
            if (string.IsNullOrEmpty(key)) return false;
            return _cache.TryGetValue(typeof(T), out Dictionary<string, Object> dictionary) && dictionary.ContainsKey(key);
        }

        internal static T Get<T>(string key) where T : Object {
            if (TryGet<T>(key, out T value)) return value;

            Debug.LogError(!_cache.ContainsKey(typeof(T))
                ? $"[Resources] type cache not found. Did you call Load<{typeof(T).Name}>()?"
                : $"[Resources] key not found in cache.");
            
            return null;
        }

        internal static bool TryGet<T>(string key, out T value) where T : Object {
            value = null;

            if (string.IsNullOrEmpty(key)) return false;
            if (!_cache.TryGetValue(typeof(T), out Dictionary<string, Object> dictionary)) return false;
            if (!dictionary.TryGetValue(key, out Object obj)) return false;

            value = obj as T;
            return value != null;
        }

        internal static IReadOnlyList<T> GetAll<T>() where T : Object {
            if (!_cache.TryGetValue(typeof(T), out Dictionary<string, Object> dictionary)) return Array.Empty<T>();
            return dictionary.Values.Select(x => x as T).Where(x => x != null).ToList();
        }

        internal static void Clear<T>() where T : Object {
            _cache.Remove(typeof(T));
        }

        internal static void ClearAll() {
            _cache.Clear();
        }

        private static string GetKey<T>(T obj, ResourceKeyPolicy policy, Func<T, string> keySelector)
            where T : Object {
            string key = keySelector != null ? keySelector(obj) : obj.name;
            if (string.IsNullOrEmpty(key)) return string.Empty;
            return policy switch {
                ResourceKeyPolicy.Name => key,
                ResourceKeyPolicy.NameLower => key.ToLowerInvariant(),
                _ => key
            };
        }

    }
}