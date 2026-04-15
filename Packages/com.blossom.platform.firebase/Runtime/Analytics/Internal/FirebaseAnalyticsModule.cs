#if SDK_FIREBASE && SDK_FIREBASE_ANALYTICS

using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Analytics;

namespace Blossom.Platform.Firebase.Internal {
    internal sealed class FirebaseAnalyticsModule : IFirebaseModule {

        #region Properties
        
        internal static bool IsInitialized { get; private set; }
        
        public string ModuleName => "FirebaseAnalytics";

        #endregion

        #region Initialize
        
        public UniTask InitializeAsync(FirebaseApp app) {
            if (IsInitialized) return UniTask.CompletedTask;

            SetCollectionEnable(true);
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        #endregion

        #region Set

        internal static void SetCollectionEnable(bool enable) {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(enable);
        }

        internal static void SetUserId(string userId) {
            FirebaseAnalytics.SetUserId(userId);
        }
        
        internal static void SetUserProperty(string name, string value) {
            if (string.IsNullOrEmpty(name)) return;
            FirebaseAnalytics.SetUserProperty(name, value ?? string.Empty);
        }

        #endregion

        #region Event

        internal static void LogEvent(string eventName) {
            if (string.IsNullOrEmpty(eventName)) return;
            FirebaseAnalytics.LogEvent(eventName);
        }

        internal static void LogEvent(string eventName, string parameterName, string parameterValue) {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(parameterName)) return;
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue ?? string.Empty);
        }

        internal static void LogEvent(string eventName, string parameterName, int parameterValue) {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(parameterName)) return;
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }

        internal static void LogEvent(string eventName, string parameterName, long parameterValue) {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(parameterName)) return;
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }
        
        internal static void LogEvent(string eventName, string parameterName, double parameterValue) {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(parameterName)) return;
            FirebaseAnalytics.LogEvent(eventName, parameterName, parameterValue);
        }
        
        internal static void LogEvent(string eventName, params Parameter[] parameters) {
            if (string.IsNullOrEmpty(eventName)) return;
            FirebaseAnalytics.LogEvent(eventName, parameters);
        }

        #endregion

    }
}

#endif