#if SDK_FIREBASE && SDK_FIREBASE_CRASHLYTICS

using System;
using Blossom.Platform.Firebase.Internal;

namespace Blossom.Platform.Firebase {
    public static class PlatformFirebaseCrashlytics {
        
        #region Properties

        /// <summary>
        /// Crashlytics 모듈 초기화 완료 여부.
        /// </summary>
        public static bool IsInitialized => FirebaseCrashlyticsModule.IsInitialized;

        #endregion

        #region Register

        /// <summary>
        /// Firebase Crashlytics 모듈을 Firebase 초기화 대상에 등록한다.
        /// </summary>
        public static void Register() => FirebaseModuleRegister.RegisterModule(new FirebaseCrashlyticsModule());

        #endregion

        #region Set

        /// <summary>
        /// Crashlytics 수집 활성화 여부 설정.
        /// </summary>
        /// <param name="enabled"></param>
        public static void SetCollectionEnabled(bool enabled) =>
            FirebaseCrashlyticsModule.SetCollectionEnabled(enabled);

        /// <summary>
        /// 사용자 Id 설정.
        /// </summary>
        /// <param name="userId"></param>
        public static void SetUserId(string userId) => FirebaseCrashlyticsModule.SetUserId(userId);

        /// <summary>
        /// 커스텀 키 설정.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetCustomKey(string key, string value) => FirebaseCrashlyticsModule.SetCustomKey(key, value);

        #endregion

        #region Log

        /// <summary>
        /// 로그 기록.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(string message) => FirebaseCrashlyticsModule.Log(message);

        /// <summary>
        /// 예외 기록.
        /// </summary>
        /// <param name="exception"></param>
        public static void RecordException(Exception exception) => FirebaseCrashlyticsModule.RecordException(exception);

        #endregion

    }
}

#endif