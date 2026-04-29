#if SDK_FIREBASE && SDK_FIREBASE_ANALYTICS

namespace Blossom.Platform.Firebase {
    using Internal;
    
    public static class PlatformFirebaseAnalyticsService {

        private static bool _registered;

        /// <summary>
        /// Blossom.Analytics에 Firebase Analytics 서비스를 등록.
        /// </summary>
        public static void Register() {
            if (_registered) return;
            Analytics.Analytics.RegisterService(new FirebaseAnalyticsService());
            _registered = true;
        }

    }
}

#endif