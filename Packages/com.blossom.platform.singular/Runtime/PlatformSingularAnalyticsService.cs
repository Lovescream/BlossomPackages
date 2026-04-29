#if SDK_SINGULAR

namespace Blossom.Platform.Singular {
    using Internal;
    
    public static class PlatformSingularAnalyticsService {

        private static bool _registered;

        /// <summary>
        /// Blossom.Analytics에 Singular Analytics 서비스를 등록.
        /// </summary>
        public static void Register() {
            if (_registered) return;
            Analytics.Analytics.RegisterService(new SingularAnalyticsService());
            _registered = true;
        }

    }
}

#endif