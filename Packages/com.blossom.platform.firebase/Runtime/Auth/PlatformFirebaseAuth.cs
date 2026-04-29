#if SDK_FIREBASE && SDK_FIREBASE_AUTH

using Firebase.Auth;

namespace Blossom.Platform.Firebase {
    using System;
    using Cysharp.Threading.Tasks;
    using Internal;
    
    public static class PlatformFirebaseAuth {

        #region Properties

        /// <summary>
        /// Auth 모듈 초기화 완료 여부.
        /// </summary>
        public static bool IsInitialized => FirebaseAuthModule.IsInitialized;

        /// <summary>
        /// FirebaseAuth 인스턴스.
        /// </summary>
        public static FirebaseAuth Auth => FirebaseAuthModule.Auth;

        /// <summary>
        /// 현재 로그인된 사용자.
        /// </summary>
        public static FirebaseUser CurrentUser => FirebaseAuthModule.CurrentUser;

        /// <summary>
        /// 현재 유효한 로그인 사용자가 존재하는지 여부.
        /// </summary>
        public static bool IsSignedIn {
            get {
                FirebaseUser user = CurrentUser;
                return user != null && user.IsValid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 현재 사용자 변경 시 호출.
        /// </summary>
        public static event Action<FirebaseUser> OnUserChanged {
            add => FirebaseAuthModule.OnUserChanged += value;
            remove => FirebaseAuthModule.OnUserChanged -= value;
        }

        /// <summary>
        /// 로그인 성공 시 호출.
        /// </summary>
        public static event Action<FirebaseUser> OnSignedIn {
            add => FirebaseAuthModule.OnSignedIn += value;
            remove => FirebaseAuthModule.OnSignedIn -= value;
        }

        /// <summary>
        /// 로그아웃 시 호출.
        /// </summary>
        public static event Action OnSignedOut {
            add => FirebaseAuthModule.OnSignedOut += value;
            remove => FirebaseAuthModule.OnSignedOut -= value;
        }

        #endregion
        
        #region Register

        /// <summary>
        /// Firebase Auth 모듈을 Firebase 초기화 대상에 등록한다.
        /// </summary>
        public static void Register() => FirebaseModuleRegister.RegisterModule(new FirebaseAuthModule());
        
        #endregion
        
        #region Auth

        /// <summary>
        /// 익명 로그인.
        /// </summary>
        /// <returns>로그인된 사용자. 실패 시 null.</returns>
        public static UniTask<FirebaseUser> SignInAnonymouslyAsync() => FirebaseAuthModule.SignInAnonymouslyAsync();

        /// <summary>
        /// 로그아웃.
        /// </summary>
        public static void SignOut() => FirebaseAuthModule.SignOut();

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 Auth Emulator 사용 호스트 설정.
        /// 예: localhost:9099
        /// </summary>
        public static void SetEmulatorHost(string host) => FirebaseAuthModule.SetEmulatorHost(host);
#endif

        #endregion

    }
}

#endif