using UnityEngine;

namespace Blossom.Core.Pooling {
    public static class Pools {

        /// <summary>
        /// prefab 인스턴스를 풀에서 꺼냄.
        /// </summary>
        public static GameObject Pop(GameObject prefab, Transform parent = null) => PoolManager.Pop(prefab, parent);
        
        /// <summary>
        /// 오브젝트를 풀로 반환.
        /// </summary>
        /// <returns>반환 성공 여부</returns>
        public static bool Release(GameObject instance, string poolKey) => PoolManager.Release(instance, poolKey);
        
        /// <summary>
        /// 특정 prefab 풀을 비움.
        /// </summary>
        /// <param name="prefab"></param>
        public static void Clear(GameObject prefab) => PoolManager.Clear(prefab);
        
        /// <summary>
        /// 모든 풀을 비움.
        /// </summary>
        public static void ClearAll() => PoolManager.ClearAll();

    }
}