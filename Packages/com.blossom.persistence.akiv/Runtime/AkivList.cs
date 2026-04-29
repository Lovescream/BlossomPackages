namespace Blossom.Persistence.Akiv {
    using System;
    using System.Collections.Generic;
    using Internal;
    using Newtonsoft.Json;
    using UnityEngine;
    
    [Serializable]
    public class AkivList<T> : IAkivWrapper {

        #region Properties

        [JsonIgnore] public IReadOnlyList<T> List => _list;
        [JsonIgnore] public int Count => _list?.Count ?? 0;

        public T this[int index] {
            get {
                if (_list == null || index < 0 || index >= _list.Count) return default;
                return _list[index];
            }
        }

        #endregion

        #region Fields

        [SerializeField, JsonProperty("List")] private List<T> _list = new();
        [NonSerialized, JsonIgnore] private AkivData _parent;

        [field: NonSerialized] public event Action OnChanged;
        [field: NonSerialized] public event Action<T> OnItemAdded;
        [field: NonSerialized] public event Action<T> OnItemRemoved;
        [field: NonSerialized] public event Action OnCleared;

        #endregion

        #region Constructor

        public AkivList() { }

        public AkivList(IEnumerable<T> list, AkivData parent) {
            _list = list != null ? new List<T>(list) : new List<T>();
            _parent = parent;
        }

        public AkivList(AkivData parent) {
            _list = new();
            _parent = parent;
        }
        
        void IAkivWrapper.Bind(AkivData parent) => _parent = parent;

        #endregion

        #region Collection
        
        public void Add(T item) {
            _list ??= new();
            _list.Add(item);

            OnItemAdded?.Invoke(item);
            MarkChanged();
        }

        public void AddRange(IEnumerable<T> items) {
            if (items == null) return;
            _list ??= new();

            bool added = false;
            foreach (T item in items) {
                _list.Add(item);
                added = true;
                OnItemAdded?.Invoke(item);
            }
            
            if (added) MarkChanged();
        }

        public bool Remove(T item) {
            if (_list == null) return false;
            
            bool removed = _list.Remove(item);
            if (!removed) return false;
            
            OnItemRemoved?.Invoke(item);
            MarkChanged();
            return true;
        }

        public void Clear() {
            if (_list == null || _list.Count == 0) return;
            
            _list.Clear();
            OnCleared?.Invoke();
            MarkChanged();
        }

        public void Modify(Action<List<T>> modifier) {
            if (modifier == null) return;

            _list ??= new();

            modifier(_list);
            MarkChanged();
        }
        
        #endregion

        private void MarkChanged() {
            OnChanged?.Invoke();
            _parent?.OnChanged();
        }

    }
}