namespace Blossom.Monetization.IAP {
    using System;
    using UnityEngine;
    
    /// <summary>
    /// IAP 태그 식별자.
    /// <para/>
    /// 태그 이름이 변경되더라도 내부 식별은 Guid 기반으로 유지할 수 있도록 한다.
    /// </summary>
    [Serializable]
    public struct IAPTagId : IEquatable<IAPTagId> {
        /// <summary>
        /// 태그 Guid 문자열.
        /// </summary>
        public string Guid => guid;
        /// <summary>
        /// 유효한 Guid를 가지고 있는지 여부.
        /// </summary>
        public bool IsValid => !string.IsNullOrEmpty(guid);
        [SerializeField] private string guid;

        public IAPTagId(string guid) => this.guid = guid;

        public bool Equals(IAPTagId other) => guid == other.guid;
        public override bool Equals(object obj) => obj is IAPTagId other && Equals(other);
        public override int GetHashCode() => guid?.GetHashCode() ?? 0;
        public override string ToString() => guid;
    }
}