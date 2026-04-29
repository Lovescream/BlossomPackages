namespace Blossom.Persistence.Akiv {
    using System;
    using System.Collections.Generic;
    using Internal;
    using Newtonsoft.Json;
    using UnityEngine;
    
    [Serializable]
    public class AkivDictionary<TKey, TValue> : IAkivWrapper {

        #region Properties

        [JsonIgnore] public IReadOnlyDictionary<TKey, TValue> Dictionary => _dictionary;
        [JsonIgnore] public int Count => _dictionary?.Count ?? 0;
        public TValue this[TKey key] {
            get => _dictionary.GetValueOrDefault(key, default);
            set => Set(key, value);
        }

        #endregion

        #region Fields

        [SerializeField, JsonProperty("Dictionary")] private Dictionary<TKey, TValue> _dictionary = new();
        [NonSerialized, JsonIgnore] private AkivData _parent;

        [field: NonSerialized] public event Action OnChanged;
        [field: NonSerialized] public event Action<TKey, TValue> OnSet; // 추가 또는 수정.
        [field: NonSerialized] public event Action<TKey> OnRemoved;
        [field: NonSerialized] public event Action OnCleared;

        #endregion

        #region Constructor

        public AkivDictionary() { }

        public AkivDictionary(IDictionary<TKey, TValue> dictionary, AkivData parent) {
            _dictionary = dictionary != null ? new Dictionary<TKey, TValue>(dictionary) : new();
            _parent = parent;
        }

        public AkivDictionary(AkivData parent) {
            _dictionary = new();
            _parent = parent;
        }

        void IAkivWrapper.Bind(AkivData parent) => _parent = parent;

        #endregion

        #region Collections

        public bool TryGetValue(TKey key, out TValue value) {
            if (_dictionary == null) {
                value = default;
                return false;
            }

            return _dictionary.TryGetValue(key, out value);
        }

        public bool ContainsKey(TKey key) => _dictionary != null && _dictionary.ContainsKey(key);

        public void Set(TKey key, TValue value) {
            _dictionary ??= new();

            bool changed = true;
            if (_dictionary.TryGetValue(key, out TValue prev)) {
                if (EqualityComparer<TValue>.Default.Equals(prev, value))
                    changed = false;
            }

            _dictionary[key] = value;
            if (!changed) return;
            
            OnSet?.Invoke(key, value);
            MarkChanged();
        }

        public bool TryAdd(TKey key, TValue value) {
            _dictionary ??= new();
            bool added = _dictionary.TryAdd(key, value);
            if (!added) return false;

            OnSet?.Invoke(key, value);
            MarkChanged();
            return true;
        }

        public bool Remove(TKey key) {
            if (_dictionary == null) return false;
            
            bool removed = _dictionary.Remove(key);
            if (!removed) return false;
            
            OnRemoved?.Invoke(key);
            MarkChanged();
            return true;
        }

        public void Clear() {
            if (_dictionary == null || _dictionary.Count == 0) return;
            
            _dictionary.Clear();
            OnCleared?.Invoke();
            MarkChanged();
        }

        public void Modify(Action<Dictionary<TKey, TValue>> modifier) {
            if (modifier == null) return;

            _dictionary ??= new();
            
            modifier(_dictionary);
            MarkChanged();
        }
        
        #endregion

        private void MarkChanged() {
            OnChanged?.Invoke();
            _parent?.OnChanged();
        }

    }
}