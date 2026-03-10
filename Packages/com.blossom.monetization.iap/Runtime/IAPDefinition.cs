using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Blossom.Monetization.IAP {
    [Serializable]
    public class IAPDefinition {

        #region Properties

        public string Key {
            get => key;
            set => key = value;
        }

        public string ID {
            get {
#if UNITY_ANDROID
                return aosID;
#elif UNITY_IOS
                return iosID;
#else
return $"UnknownPlatform_{key}";
#endif
            }
        }

        public ProductType Type {
            get => type;
            set => type = value;
        }
        
        public IReadOnlyList<IAPTagId> Tags => tags;

        #endregion

        #region Fields

        [SerializeField] private string key;
        [SerializeField] private string aosID;
        [SerializeField] private string iosID;
        [SerializeField] private ProductType type;
        [SerializeField] private List<IAPTagId> tags = new();

        #endregion

        #region Constructor

        public IAPDefinition() { }

        public IAPDefinition(string key, string id, ProductType type) {
            this.key = key;
            this.aosID = id;
            this.iosID = id;
            this.type = type;
        }

        public IAPDefinition(string key, string aosID, string iosID, ProductType type) {
            this.key = key;
            this.aosID = aosID;
            this.iosID = iosID;
            this.type = type;
        }

        #endregion

    }
}