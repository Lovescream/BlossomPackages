namespace Blossom.Monetization.IAP {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Purchasing;
    
    /// <summary>
    /// IAP 상품 정의 데이터.
    /// <para/>
    /// 상품 Key, 플랫폼별 Product ID, 상품 타입, 태그 정보를 보관한다.
    /// </summary>
    [Serializable]
    public class IAPDefinition {

        #region Properties

        /// <summary>
        /// 프로젝트 내부에서 사용하는 상품 Key.
        /// </summary>
        public string Key {
            get => key;
            set => key = value;
        }

        /// <summary>
        /// 현재 플랫폼 기준으로 사용할 스토어 Product ID.
        /// </summary>
        public string Id {
            get {
#if UNITY_ANDROID
                return aosId;
#elif UNITY_IOS
                return iosId;
#else
return $"UnknownPlatform_{key}";
#endif
            }
        }

        /// <summary>
        /// 상품 타입.
        /// </summary>
        public ProductType Type {
            get => type;
            set => type = value;
        }
        
        /// <summary>
        /// 이 상품에 부여된 태그 목록.
        /// </summary>
        public IReadOnlyList<IAPTagId> Tags => tags;

        #endregion

        #region Fields

        [SerializeField] private string key;
        [SerializeField] private string aosId;
        [SerializeField] private string iosId;
        [SerializeField] private ProductType type;
        [SerializeField] private List<IAPTagId> tags = new();

        #endregion

        #region Constructor

        public IAPDefinition() { }

        public IAPDefinition(string key, string id, ProductType type) {
            this.key = key;
            this.aosId = id;
            this.iosId = id;
            this.type = type;
        }

        public IAPDefinition(string key, string aosId, string iosId, ProductType type) {
            this.key = key;
            this.aosId = aosId;
            this.iosId = iosId;
            this.type = type;
        }

        #endregion

    }
}