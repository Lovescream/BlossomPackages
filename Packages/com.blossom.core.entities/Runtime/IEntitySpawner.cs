namespace Blossom.Core.Entities {
    using UnityEngine;
    
    public interface IEntitySpawner {
        /// <summary>
        /// prefab을 기반으로 인스턴스를 생성.
        /// </summary>
        /// <param name="prefab">생성할 prefab</param>
        /// <param name="parent">부모 Transform(선택)</param>
        /// <param name="usePooling">풀링 적용 여부</param>
        /// <returns>생성된 인스턴스</returns>
        GameObject Spawn(GameObject prefab, Transform parent = null, bool usePooling = true);

        /// <summary>
        /// 인스턴스를 반환 또는 제거.
        /// </summary>
        /// <param name="instance">반환할 인스턴스</param>
        /// <param name="poolKey">풀링 키</param>
        void Despawn(GameObject instance, string poolKey);
    }
}