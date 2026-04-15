#if SDK_FIREBASE

using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Blossom.Platform.Firebase.Internal;

namespace Blossom.Platform.Firebase {
    public static class PlatformFirebase {

        #region Properties

        /// <summary>
        /// Firebase 전체 초기화 완료 여부.
        /// </summary>
        public static bool IsInitialized => FirebaseInitializer.IsInitialized;

        /// <summary>
        /// Firebase 초기화 진행 중 여부.
        /// </summary>
        public static bool IsInitializing => FirebaseInitializer.IsInitializing;

        /// <summary>
        /// 기본 FirebaseApp 인스턴스.
        /// </summary>
        public static FirebaseApp App => FirebaseInitializer.App;

        #endregion

        #region Events

        /// <summary>
        /// Firebase 초기화 완료 시 호출되는 이벤트.
        /// </summary>
        public static event Action OnInitialized {
            add => FirebaseInitializer.OnInitialized += value;
            remove => FirebaseInitializer.OnInitialized -= value;
        }

        #endregion

        #region Initialize

        /// <summary>
        /// Firebase를 초기화한다.
        /// </summary>
        /// <param name="onInitializeComplete">초기화 완료 후 호출할 콜백</param>
        public static void Initialize(Action onInitializeComplete = null) {
            FirebaseInitializer.Initialize(onInitializeComplete);
        }

        /// <summary>
        /// Firebase를 비동기적으로 초기화한다.
        /// </summary>
        /// <returns>초기화 성공 여부</returns>
        public static UniTask<bool> InitializeAsync() {
            return FirebaseInitializer.InitializeAsync();
        }
        
        #endregion

    }
}

#endif