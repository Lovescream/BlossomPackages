#if SDK_GAMEANALYTICS

namespace Blossom.Platform.GameAnalytics {
    using System;
    using Internal;
    
    public static class PlatformGameAnalytics {
        public static bool IsInitialized => GameAnalyticsInitializer.IsInitialized;

        public static void Initialize(Action onComplete = null) => GameAnalyticsInitializer.Initialize(onComplete);
    }
}

#endif