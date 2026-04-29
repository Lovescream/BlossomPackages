#if SDK_GAMEANALYTICS


namespace Blossom.Platform.GameAnalytics {
    using Internal;
    
    public static class PlatformGameAnalyticsService {

        private static bool _registered;

        /// <summary>
        /// Blossom.Analytics에 GameAnalytics 서비스를 등록.
        /// </summary>
        public static void Register() {
            if (_registered) return;
            Analytics.Analytics.RegisterService(new GameAnalyticsService());
            _registered = true;
        }
        
    }
}

#endif