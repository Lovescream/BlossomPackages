// TODO:: SetVisible(bool)과 같은 API 제공하기.

namespace Blossom.Core.Entities {
    using UnityEngine;
    
    /// <summary>
    /// 게임 내에서 관리되는 엔티티의 범용 베이스 클래스.
    /// </summary>
    public abstract class Entity : MonoBehaviour {

        internal string PoolKey { get; private set; }

        private bool _initialized;

        protected virtual void Awake() => Initialize();
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void LateUpdate() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnDestroy() => OnRelease();
        
        public bool Initialize() {
            if (_initialized) return false;
            OnInitialize();
            _initialized = true;
            return true;
        }

        internal void SetPoolKey(string key) => PoolKey = key ?? string.Empty;

        /// <summary>
        /// 최초 초기화 시점에 실행되는 동작.
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 오브젝트 초기화 동작. 풀에서 Pop되어 등장할 때마다 호출됨.
        /// </summary>
        public virtual void OnSpawned() { }
        
        /// <summary>
        /// 오브젝트 반환 동작. 풀에 반환되기 적전에 호출됨. 여러 번 호출될 수 있으므로(멱등성) 중복 호출에 안전하게 구현되어야 함.
        /// </summary>
        public virtual void OnRelease() { }

    }
    
}