namespace Blossom.Monetization.IAP {
    using System;
    using UnityEngine;
    
    /// <summary>
    /// IAP 태그 정의 데이터.
    /// <para/>
    /// 외부 식별용 이름과 내부 식별용 Guid를 함께 보관한다.
    /// </summary>
    [Serializable]
    public class IAPTagDefinition {
        /// <summary>
        /// 내부 식별용 Guid.
        /// </summary>
        public string Guid => guid;
        /// <summary>
        /// 식별용 태그 이름.
        /// </summary>
        public string Name => name;
        [SerializeField] private string guid;
        [SerializeField] private string name;

#if UNITY_EDITOR
        public static IAPTagDefinition Create(string name) {
            return new IAPTagDefinition {
                guid = System.Guid.NewGuid().ToString("N"),
                name = name?.Trim()
            };
        }

        public void SetName(string newName) => name = newName?.Trim();
#endif
    }
}