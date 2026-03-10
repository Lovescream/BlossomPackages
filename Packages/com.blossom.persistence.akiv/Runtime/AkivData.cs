using System;
using System.Reflection;
using System.Runtime.Serialization;
using Blossom.Persistence.Akiv.Internal;

namespace Blossom.Persistence.Akiv {
    [Serializable]
    public class AkivData : IAkivData {
        public string Key {
            get => _key;
            set => _key = value;
        }
        [NonSerialized] private string _key;

        public AkivData() {
            _key = this.GetType().Name;
            RebindAkivValues();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context) {
            if (string.IsNullOrEmpty(_key)) _key = this.GetType().Name;
            RebindAkivValues();
        }

        protected void RebindAkivValues() {
            const BindingFlags flags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            
            for (Type type = GetType(); type != null && type != typeof(object); type = type.BaseType) {
                foreach (FieldInfo fieldInfo in type.GetFields(flags)) {
                    if (Attribute.IsDefined(fieldInfo, typeof(NonSerializedAttribute))) continue;

                    object value = fieldInfo.GetValue(this);
                    if (value is not IAkivWrapper wrapper) continue;

                    wrapper.Bind(this);
                }
            }
        }

        public virtual void Flush() { }
        
        public virtual void Clear() { }

        public virtual void OnChanged() {
            AkivSystem.OnChanged(_key);
        }
    }
}