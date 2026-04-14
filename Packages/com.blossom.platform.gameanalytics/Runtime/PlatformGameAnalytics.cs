#if gameanalytics_max_enabled

using System;
using Blossom.Platform.GameAnalytics.Internal;

namespace Blossom.Platform.GameAnalytics {
    public static class PlatformGameAnalytics {
        public static bool IsInitialized => GameAnalyticsInitializer.IsInitialized;

        public static void Initialize(Action onComplete = null) => GameAnalyticsInitializer.Initialize(onComplete);
    }
}

#endif