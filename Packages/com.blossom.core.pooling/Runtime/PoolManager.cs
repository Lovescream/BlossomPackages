namespace Blossom.Core.Pooling {
    using System.Collections.Generic;
    using UnityEngine;
    
    internal static class PoolManager {
        
        private static readonly Dictionary<string, Pool> _pools = new();
        private static Transform _root;
        private static Transform Root {
            get {
                if (_root != null) return _root;
                GameObject obj = new("[Pool_Root]");
                Object.DontDestroyOnLoad(obj);
                _root = obj.transform;
                return _root;
            }
        }

        internal static GameObject Pop(GameObject prefab, Transform parent = null) {
            if (prefab == null) {
                Nyo.Error($"Prefab is null.");
                return null;
            }

            Pool pool = GetOrCreatePool(prefab);
            GameObject obj = pool.Pop();
            if (parent != null) obj.transform.SetParent(parent, worldPositionStays: false);

            return obj;
        }

        internal static bool Release(GameObject instance, string poolKey) {
            if (instance == null) return false;
            if (string.IsNullOrEmpty(poolKey)) return false;
            if (!_pools.TryGetValue(poolKey, out Pool pool)) return false;

            pool.Push(instance);
            return true;
        }

        internal static void Clear(GameObject prefab) {
            if (prefab == null) return;
            if (!_pools.TryGetValue(prefab.name, out Pool pool)) return;
            pool.Clear();
            _pools.Remove(prefab.name);
        }

        internal static void ClearAll() {
            foreach (Pool pool in _pools.Values) pool.Clear();
            _pools.Clear();
        }

        private static Pool GetOrCreatePool(GameObject prefab) {
            if (_pools.TryGetValue(prefab.name, out Pool pool)) return pool;
            pool = new(prefab, Root);
            _pools.Add(prefab.name, pool);
            return pool;
        }

    }
}