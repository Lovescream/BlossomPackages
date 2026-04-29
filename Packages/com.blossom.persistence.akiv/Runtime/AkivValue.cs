namespace Blossom.Persistence.Akiv {
    using System;
    using System.Collections.Generic;
    using Internal;
    using Newtonsoft.Json;
    using UnityEngine;
    
    [Serializable]
    public class AkivValue<T> : IAkivWrapper where T : struct {

        #region Properties

        [JsonIgnore]
        public T Value {
            get => _value;
            set {
                T preValue = _value;
                _value = value;
                if (EqualityComparer<T>.Default.Equals(preValue, _value)) return;
                int compare = Comparer<T>.Default.Compare(_value, preValue);
                if (compare < 0) OnValueDecreased?.Invoke(_value);
                else if (compare > 0) OnValueIncreased?.Invoke(_value);
                OnValueChanged?.Invoke(_value);
                _parent?.OnChanged();
            }
        }

        [JsonIgnore]
        public T DisplayValue {
            get=>_displayValue;
            set {
                T previousValue = _displayValue;
                _displayValue = value;
                
                if (!EqualityComparer<T>.Default.Equals(previousValue, _displayValue))
                    OnDisplayValueChanged?.Invoke(_displayValue);
            }
        }

        #endregion
    
        #region Fields

        [SerializeField, JsonProperty("Value")] private T _value;
        [NonSerialized, JsonIgnore] private T _displayValue;
        [NonSerialized, JsonIgnore] private AkivData _parent;
    
        [field: NonSerialized] public event Action<T> OnValueChanged;
        [field: NonSerialized] public event Action<T> OnValueDecreased;
        [field: NonSerialized] public event Action<T> OnValueIncreased;
        [field: NonSerialized] public event Action<T> OnDisplayValueChanged;

        #endregion

        #region Constructor
        
        public AkivValue() { }

        public AkivValue(T value, AkivData parent) {
            _value = value;
            _displayValue = value;
            _parent = parent;
        }

        void IAkivWrapper.Bind(AkivData parent) {
            _parent = parent;
            _displayValue = _value;
        }

        #endregion
        
        public void DisplayValueSync() {
            _displayValue = Value;
            OnDisplayValueChanged?.Invoke(_displayValue);
        }

    }
}