#if SDK_FIREBASE && SDK_FIREBASE_ANALYTICS

using Firebase.Analytics;
using Blossom.Platform.Firebase.Internal;

namespace Blossom.Platform.Firebase {
    public static class PlatformFirebaseAnalytics {

        #region Properties

        public static bool IsInitialized => FirebaseAnalyticsModule.IsInitialized;

        #endregion

        #region Register

        /// <summary>
        /// Firebase Analytics 모듈을 Firebase 초기화 대상에 등록한다.
        /// </summary>
        public static void Register() => FirebaseModuleRegister.RegisterModule(new FirebaseAnalyticsModule());
        
        #endregion

        #region Set

        /// <summary>
        /// Analytics 수집 활성화 여부 설정.
        /// </summary>
        public static void SetCollectionEnabled(bool enabled) => FirebaseAnalyticsModule.SetCollectionEnable(enabled);

        /// <summary>
        /// 사용자 ID 설정.
        /// </summary>
        public static void SetUserId(string userId) => FirebaseAnalyticsModule.SetUserId(userId);

        /// <summary>
        /// 사용자 속성 설정.
        /// </summary>
        public static void SetUserProperty(string name, string value) =>
            FirebaseAnalyticsModule.SetUserProperty(name, value);

        #endregion
        
        #region Event

        /// <summary>
        /// 파라미터 없이 이벤트 전송.
        /// </summary>
        public static void LogEvent(string eventName) => FirebaseAnalyticsModule.LogEvent(eventName);

        /// <summary>
        /// 단일 string 파라미터 이벤트 전송.
        /// </summary>
        public static void LogEvent(string eventName, string parameterName, string parameterValue) =>
            FirebaseAnalyticsModule.LogEvent(eventName, parameterName, parameterValue);

        /// <summary>
        /// 단일 int 파라미터 이벤트 전송.
        /// </summary>
        public static void LogEvent(string eventName, string parameterName, int parameterValue) =>
            FirebaseAnalyticsModule.LogEvent(eventName, parameterName, parameterValue);

        /// <summary>
        /// 단일 long 파라미터 이벤트 전송.
        /// </summary>
        public static void LogEvent(string eventName, string parameterName, long parameterValue) =>
            FirebaseAnalyticsModule.LogEvent(eventName, parameterName, parameterValue);

        /// <summary>
        /// 단일 double 파라미터 이벤트 전송.
        /// </summary>
        public static void LogEvent(string eventName, string parameterName, double parameterValue) =>
            FirebaseAnalyticsModule.LogEvent(eventName, parameterName, parameterValue);

        /// <summary>
        /// 여러 파라미터 이벤트 전송.
        /// </summary>
        public static void LogEvent(string eventName, params Parameter[] parameters) =>
            FirebaseAnalyticsModule.LogEvent(eventName, parameters);

        #endregion

    }
}

#endif