namespace Blossom.Core.Entities {
    using UnityEngine;

    public static class Entities {

        public static bool IsInitialized { get; private set; }

        /// <summary>
        /// Entity 시스템 초기화.
        /// <para/>
        /// 반드시 PrefabProvider / Spawner를 제공해야 함.
        /// </summary>
        /// <param name="provider">prefab 키를 바탕으로 prefab을 제공해주는 provider</param>
        /// <param name="spawner">실제 생성/반환을 담당하는 spawner</param>
        public static void Initialize(IPrefabProvider provider, IEntitySpawner spawner) {
            if (IsInitialized) return;
            EntityManager.Initialize(provider, spawner);
            IsInitialized = true;
        }

        /// <summary>
        /// prefab 키를 기반으로 Entity 생성.
        /// <para/>
        /// key가 null/empty이면 typeof(T).Name을 기본 키로 사용.
        /// </summary>
        /// <param name="key">prefab 키(선택)</param>
        /// <param name="parent">부모 Transform(선택)</param>
        /// <typeparam name="T">Entity 컴포넌트 타입</typeparam>
        /// <returns>생성된 Entity, 실패 시 null</returns>
        public static T Spawn<T>(string key = null, Transform parent = null) where T : Entity =>
            EntityManager.Spawn<T>(key, parent);

        /// <summary>
        /// Entity를 반환/제거.
        /// </summary>
        /// <param name="entity">반환할 Entity</param>
        /// <typeparam name="T">Entity 타입</typeparam>
        public static void Despawn<T>(T entity) where T : Entity => EntityManager.Despawn(entity);

        /// <summary>
        /// 현재 활성화된 모든 Entity를 반환/제거.
        /// </summary>
        public static void ClearAll() => EntityManager.ClearAll();



    }
}