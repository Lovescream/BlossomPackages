using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blossom.Core.Entities {
    internal static class EntityManager {

        private static IPrefabProvider _provider;
        private static IEntitySpawner _spawner;

        // 활성 상태로 관리 중인 엔티티들.
        private static readonly HashSet<Entity> _entities = new();

        internal static void Initialize(IPrefabProvider provider, IEntitySpawner spawner) {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        }

        internal static T Spawn<T>(string key, Transform parent = null) where T : Entity {
            if (_provider == null || _spawner == null) {
                Debug.LogError($"[EntityManager] Spawn<{typeof(T).Name}>({key}): Not initialized.");
                return null;
            }

            if (string.IsNullOrEmpty(key)) key = typeof(T).Name;

            GameObject prefab = _provider.Provide(key);
            if (prefab == null) {
                Debug.LogError($"[EntityManager] Spawn<{typeof(T).Name}>({key}): prefab not found.");
                return null;
            }

            GameObject instance = _spawner.Spawn(prefab, parent);
            if (instance == null) {
                Debug.LogError($"[EntityManager] Spawn<{typeof(T).Name}>({key}): failed to spawn instance.");
                return null;
            }

            if (!instance.TryGetComponent(out T entity) || entity == null) {
                Debug.LogError($"[EntityManager] Spawn<{typeof(T).Name}>({key}): component not found on instance.");
                _spawner.Despawn(instance, key);
                return null;
            }

            entity.Initialize();
            entity.SetPoolKey(key);
            _entities.Add(entity);
            entity.OnSpawned();
            return entity;
        }

        internal static void Despawn<T>(T entity) where T : Entity {
            if (entity == null) return;
            _entities.Remove(entity);
            entity.OnRelease();
            _spawner?.Despawn(entity.gameObject, entity.PoolKey);
        }

        internal static void ClearAll() {
            if (_spawner == null) {
                _entities.Clear();
                return;
            }

            List<Entity> entities = new(_entities);
            _entities.Clear();
            foreach (Entity entity in entities) {
                if (entity == null) continue;
                entity.OnRelease();
                _spawner.Despawn(entity.gameObject, entity.PoolKey);
            }
        }

    }
}