#if SDK_GAMEANALYTICS

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blossom.Platform.GameAnalytics.Internal {
    internal static class GameAnalyticsInitializer {

        #region Properties

        internal static bool IsInitialized => GameAnalyticsSDK.GameAnalytics.Initialized;

        #endregion
        
        #region Fields

        private static bool _isInitializing;
        private static GameObject _gameAnalyticsObject;
        private static Action _onInitializeCompleted;

        
        #endregion
        
        #region Initialize

        internal static void Initialize(Action onComplete = null) {
            if (IsInitialized) {
                onComplete?.Invoke();
                return;
            }

            if (onComplete != null) {
                _onInitializeCompleted += onComplete;
            }

            if (_isInitializing) return;

            InitializeAsync().Forget();
        }

        private static async UniTaskVoid InitializeAsync() {
            _isInitializing = true;

            try {
                if (_gameAnalyticsObject == null) {
                    GameObject prefab = Resources.Load<GameObject>($"GameAnalytics");
                    if (prefab == null) {
                        Debug.LogError($"[Blossom:GameAnalytics] GameAnalytics prefab could not be loaded.");
                        _onInitializeCompleted = null;
                        return;
                    }
                    _gameAnalyticsObject = Object.Instantiate(prefab);
                    _gameAnalyticsObject.name = "[Platform] GameAnalytics";
                    Object.DontDestroyOnLoad(_gameAnalyticsObject);
                }
                
                GameAnalyticsSDK.GameAnalytics.Initialize();

                try {
                    await UniTask.WaitUntil(() => GameAnalyticsSDK.GameAnalytics.Initialized)
                        .Timeout(TimeSpan.FromSeconds(10));
                }
                catch (TimeoutException) {
                    Debug.LogError("[Blossom:GameAnalytics] GameAnalytics initialization timed out");
                    _onInitializeCompleted = null;
                    return;
                }
                
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