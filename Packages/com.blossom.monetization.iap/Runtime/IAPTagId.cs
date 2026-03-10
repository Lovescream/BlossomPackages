using System;
using UnityEngine;

namespace Blossom.Monetization.IAP {
    [Serializable]
    public struct IAPTagId : IEquatable<IAPTagId> {
        public string Guid => guid;
        public bool IsValid => !string.IsNullOrEmpty(guid);
        [SerializeField] private string guid;

        public IAPTagId(string guid) => this.guid = guid;

        public bool Equals(IAPTagId other) => guid == other.guid;
        public override bool Equals(object obj) => obj is IAPTagId other && Equals(other);
        public override int GetHashCode() => guid?.GetHashCode() ?? 0;
        public override string ToString() => guid;
    }
}