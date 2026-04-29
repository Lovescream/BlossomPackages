namespace Blossom.Presentation.Receive {
    using System;
    
    /// <summary>
    /// 보상 식별자.
    /// Ex: (Group: Currency, Key: Gold), (Group: Life, Key: Heart) (Group: Item, Key: Magnet)
    /// </summary>
    [Serializable]
    public readonly struct ReceiveKey : IEquatable<ReceiveKey> {

        public string Group { get; }
        public string Key { get; }

        public bool IsValid => !string.IsNullOrEmpty(Group) && !string.IsNullOrEmpty(Key);

        public ReceiveKey(string group, string key) {
            Group = group ?? string.Empty;
            Key = key ?? string.Empty;
        }

        public bool Equals(ReceiveKey other) {
            return string.Equals(Group, other.Group, StringComparison.Ordinal)
                   && string.Equals(Key, other.Key, StringComparison.Ordinal);
        }

        public override bool Equals(object obj) {
            return obj is ReceiveKey other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(Group, Key);
        }

        public override string ToString() {
            return $"{Group}:{Key}";
        }

        public static bool operator ==(ReceiveKey left, ReceiveKey right) => left.Equals(right);
        public static bool operator !=(ReceiveKey left, ReceiveKey right) => !left.Equals(right);
    }
}