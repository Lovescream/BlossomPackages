#if SDK_SINGULAR

using System;
using Blossom.Common;
using Cysharp.Threading.Tasks;
using Singular;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blossom.Platform.Singular.Internal {
    internal static class SingularInitializer {

        #region Properties

        internal static bool IsInitialized {
            get {
#if UNITY_EDITOR
                return true;
#else
                return SingularSDK.Initialized;
#endif
            }
        }

        #endregion

        #region Fields

        private static SingularSettingsSO _settings;
        private static bool _isInitializing;
        private static GameObject _singularObject;
        private static Action _onInitializeCompleted;

        #endregion

        #region Initialize

        internal static void Initialize(SingularSettingsSO settings = null, Action onComplete = null) {
            if (IsInitialized) {
                onComplete?.Invoke();
                return;
            }

            if (onComplete != null) {
                _onInitializeCompleted += onComplete;
            }

            if (_isInitializing) return;

            _settings = settings ?? SettingsLoader.Get<SingularSettingsSO>();
            if (_settings == null) {
                Debug.LogError("[Blossom:Singular] SingularSettingsSO not found.");
                _onInitializeCompleted = null;
                return;
            }

            InitializeAsync().Forget();
        }

        private static async UniTaskVoid InitializeAsync() {
            _isInitializing = true;

            try {
                if (_singularObject == null) {
                    GameObject prefab = Resources.Load<GameObject>("SingularSDK");
                    if (prefab == null) {
                        Debug.LogError("[Blossom:Singular] SingularSDK prefab could not be loaded.");
                        _onInitializeCompleted = null;
                        return;
                    }

                    _singularObject = Object.Instantiate(prefab);
                    _singularObject.name = "[Platform] SingularSDK";
                    Object.DontDestroyOnLoad(_singularObject);
                }

                if (!_singularObject.TryGetComponent(out SingularSDK sdk) || sdk == null) {
                    Debug.LogError("[Blossom:Singular] SingularSDK component not found.");
                    _onInitializeCompleted = null;
                    return;
                }

                sdk.SingularAPIKey = _settings.ApiKey;
                sdk.SingularAPISecret = _settings.ApiSecret;
                sdk.enableLogging = _settings.EnableLogging;
                sdk.logLevel = _settings.LogLevel;

                SingularSDK.InitializeSingularSDK();

#if !UNITY_EDITOR
                try {
                    await UniTask.WaitUntil(() => SingularSDK.Initialized)
                        .Timeout(TimeSpan.FromSeconds(_settings.InitializeTimeout));
                }
                catch (TimeoutException) {
                    Debug.LogError("[Blossom:Singular] Initialize timeout.");
                    _onInitializeCompleted = null;
                    return;
                }
#endif

                Action callback = _onInitializeCompleted;
                _onInitializeCompleted = null;
                callback?.Invoke();
            }
            finally {
                _isInitializing = false;
            }
        }

        #endregion

    }
}

#endif