using UnityEngine;

namespace Blossom.Core.UI {
    public abstract class UIBase : MonoBehaviour {

        internal string PoolKey { get; private set; }

        public RectTransform RectTransform {
            get {
                Initialize();
                return _rect;
            }
        }
        
        protected RectTransform _rect;
        private bool _initialized;

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void OnEnable() => Initialize();
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() => OnRelease();
        
        public virtual bool Initialize() {
            if (_initialized) return false;
            _initialized = true;

            if (this.TryGetComponent<Canvas>(out _)) {
                _rect = this.transform.childCount > 0
                    ? this.transform.GetChild(0).GetComponent<RectTransform>()
                    : this.GetComponent<RectTransform>();
            }
            else _rect = this.GetComponent<RectTransform>();
            
            OnInitialize();
            return true;
        }
        
        internal void SetPoolKey(string key) => PoolKey = key ?? string.Empty;
        
        /// <summary>
        /// 최초 초기화 시점에 실행되는 동작.
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// UI가 생성되거나 풀에서 다시 꺼내졌을 때마다 호출되는 동작.
        /// </summary>
        public virtual void OnSpawned() { }
        
        /// <summary>
        /// UI가 반환/제거되기 직전에 호출됨.
        /// </summary>
        public virtual void OnRelease() { }

    }
}