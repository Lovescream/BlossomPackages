#if SDK_SINGULAR

using Blossom.Common;
using UnityEngine;

namespace Blossom.Platform.Singular {
    public class SingularSettingsSO : SettingsSOBase {

        #region Properties

        public string ApiKey => apiKey;
        public string ApiSecret => apiSecret;
        public bool EnableLogging => enableLogging;
        public int LogLevel => logLevel;
        public float InitializeTimeout => initializeTimeout;

        #endregion
        
        #region Fields

        [SerializeField] private string apiKey;
        [SerializeField] private string apiSecret;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private int logLevel = 3;
        [SerializeField] private float initializeTimeout = 10;

        #endregion

    }
}

#endif