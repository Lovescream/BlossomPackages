#if SDK_FIREBASE && SDK_FIREBASE_CRASHLYTICS

using Firebase;
using Firebase.Crashlytics;

namespace Blossom.Platform.Firebase.Internal {
    using System;
    using Cysharp.Threading.Tasks;
    
    internal sealed class FirebaseCrashlyticsModule : IFirebaseModule {

        #region Properties
        
        internal static bool IsInitialized { get; private set; }
        
        public string ModuleName => "FirebaseCrashlytics";

        #endregion

        #region Initialize
        
        public UniTask InitializeAsync(FirebaseApp app) {
            if (IsInitialized) return UniTask.CompletedTask;
            
            SetCollectionEnabled(true);
            IsInitialized = true;
            return UniTask.CompletedTask;
        }

        #endregion

        #region Set

        internal static void SetCollectionEnabled(bool enabled) {
            Crashlytics.IsCrashlyticsCollectionEnabled = enabled;
            Crashlytics.ReportUncaughtExceptionsAsFatal = enabled;
        }

        internal static void SetUserId(string userId) {
            Crashlytics.SetUserId(userId ?? string.Empty);
        }

        internal static void SetCustomKey(string key, string value) {
            if (string.IsNullOrEmpty(key)) return;
            Crashlytics.SetCustomKey(key, value ?? string.Empty);
        }
        
        #endregion

        #region Log

        internal static void Log(string message) {
            if (string.IsNullOrEmpty(message)) return;
            Crashlytics.Log(message);
        }

        internal static void RecordException(Exception exception) {
            if (exception == null) return;
            Crashlytics.LogException(exception);
        }

        #endregion

    }
}

#endif