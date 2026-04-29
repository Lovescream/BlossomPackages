namespace Blossom.Persistence.Akiv {
    using System;
    
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class AkivAttribute : Attribute {
        public string Key { get; }
        public bool UseEncryption { get; }

        public AkivAttribute(string key = null, bool useEncryption = false) {
            Key = key;
            UseEncryption = useEncryption;
        }
    }
}