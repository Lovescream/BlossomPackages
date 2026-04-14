#if gameanalytics_max_enabled

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blossom.Platform.GameAnalytics.Internal {
    internal class GameAnalyticsInitializer {

        #region Properties

        internal static bool IsInitialized => GameAnalyticsSDK.GameAnalytics.Initialized;

        #endregion

        #region Fields


        
        #endregion
        
        #region Initialize

        internal static void Initialize(Action onComplete = null) {
            if (IsInitialized) {
                onComplete?.Invoke();
                return;
            }

            GameObject prefab = Resources.Load<GameObject>($"GameAnalytics");
            if (prefab == null) {
                Debug.LogError($"[Blossom:GameAnalytics] GameAnalytics prefab could not be loaded.");
                return;
            }

            GameObject gameAnalyticsObject = Object.Instantiate(prefab);
            gameAnalyticsObject.name = "[Platform] GameAnalytics";
            
            GameAnalyticsSDK.GameAnalytics.Initialize();
        }
        
        #endregion

    }
}

#endif