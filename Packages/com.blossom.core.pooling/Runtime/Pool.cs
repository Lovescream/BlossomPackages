namespace Blossom.Core.Pooling {
    using System.Collections.Generic;
    using UnityEngine;
    
    internal sealed class Pool {
        
        private readonly GameObject _prefab;
        private readonly Stack<GameObject> _stack = new();
        private Transform _root;

        internal Pool(GameObject prefab, Transform topRoot) {
            _prefab = prefab;

            GameObject rootObject = new($"[Pool] {prefab.name}");
            rootObject.transform.SetParent(topRoot, worldPositionStays: false);
            _root = rootObject.transform;
        }

        internal GameObject Pop() {
            GameObject obj = _stack.Count > 0 ? _stack.Pop() : CreateNew();
            OnGet(obj);
            return obj;
        }

        internal void Push(GameObject obj) {
            if (obj == null) return;
            OnRelease(obj);
            _stack.Push(obj);
        }

        internal void Clear() {
            while (_stack.Count > 0) {
                GameObject obj = _stack.Pop();
                if (obj != null) Object.Destroy(obj);
            }
        }

        private GameObject CreateNew() {
            GameObject obj = Object.Instantiate(_prefab, _root);
            obj.name = _prefab.name;
            
            return obj;
        }

        private void OnGet(GameObject obj) {
            if (obj == null) return;
            obj.SetActive(true);
        }

        private void OnRelease(GameObject obj) {
            if (obj == null) return;
            obj.transform.SetParent(_root, worldPositionStays: false);
            obj.SetActive(false);
        }
        
    }
}