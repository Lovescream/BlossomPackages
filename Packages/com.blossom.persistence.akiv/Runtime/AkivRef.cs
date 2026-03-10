using System;
using System.Collections.Generic;
using Blossom.Persistence.Akiv.Internal;
using Newtonsoft.Json;
using UnityEngine;

namespace Blossom.Persistence.Akiv {
    [Serializable]
    public class AkivRef<T> : IAkivWrapper where T : class {

        #region Properties

        [JsonIgnore]
        public T Value {
            get => _value;
            set {
                T prev = _value;
                _value = value;
                if (EqualityComparer<T>.Default.Equals(prev, _value)) return;
                OnValueChanged?.Invoke(_value);
                _parent?.OnChanged();
            }
        }

        #endregion

        #region Fields

        [SerializeField, JsonProperty("Value")] private T _value;
        [NonSerialized, JsonIgnore] private AkivData _parent;

        [field: NonSerialized] public event Action<T> OnValueChanged;

        #endregion
        
        #region Constructor
        
        public AkivRef() { }

        public AkivRef(T value, AkivData parent) {
            _value = value;
            _parent = parent;
        }
        
        void IAkivWrapper.Bind(AkivData parent) => _parent = parent;
        
        #endregion
        
    }
}