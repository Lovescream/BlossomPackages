#if SDK_FIREBASE && SDK_FIREBASE_AUTH

using System;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using UnityEngine;

namespace Blossom.Platform.Firebase.Internal {
    internal sealed class FirebaseAuthModule : IFirebaseModule {

        #region Properties

        internal static bool IsInitialized { get; private set; }
        internal static FirebaseAuth Auth { get; private set; }
        internal static FirebaseUser CurrentUser { get; private set; }

        public string ModuleName => "FirebaseAuth";

        #endregion

        #region Events

        internal static event Action<FirebaseUser> OnUserChanged;
        internal static event Action<FirebaseUser> OnSignedIn;
        internal static event Action OnSignedOut;

        #endregion

        #region Initialize

        public async UniTask InitializeAsync(FirebaseApp app) {
            if (IsInitialized) return;

            Auth = FirebaseAuth.DefaultInstance;
            if (Auth == null) throw new InvalidOperationException("FirebaseAuth.DefaultInstance is null.");

#if UNITY_EDITOR
            SetEmulatorHost("localhost:9099");
#endif

            Auth.StateChanged -= OnAuthStateChanged;
            Auth.StateChanged += OnAuthStateChanged;
            OnAuthStateChanged(Auth, EventArgs.Empty);

#if UNITY_IOS && !UNITY_EDITOR
            await UniTask.Delay(300);
#endif

            FirebaseUser user = await SignInAnonymouslyInternalAsync();
            if (user == null) throw new InvalidOperationException("Anonymous sign-in failed.");

            IsInitialized = true;
        }

        #endregion

        #region Sign-In (Anonymously)
        
        private static async UniTask<FirebaseUser> SignInAnonymouslyInternalAsync() {
            if (Auth == null) return null;

            try {
                AuthResult result = await Auth.SignInAnonymouslyAsync().AsUniTask();
                FirebaseUser user = result?.User;
                if (user != null) Debug.Log($"[Blossom:Firebase:Auth] Signed in anonymously: {user.UserId}");
                return user;
            }
            catch (Exception e) {
                Debug.LogError($"[Blossom:Firebase:Auth] Anonymous sign-in failed: {e}");
                return null;
            }
        }

        internal static async UniTask<FirebaseUser> SignInAnonymouslyAsync() {
            if (!PlatformFirebase.IsInitialized) {
                bool initialized = await PlatformFirebase.InitializeAsync();
                if (!initialized) return null;
            }

            if (!IsInitialized || Auth == null) {
                Debug.LogError("[Blossom:Firebase:Auth] Auth module is not initialized.");
                return null;
            }

            if (CurrentUser != null && CurrentUser.IsValid())
                return CurrentUser;

            try {
                AuthResult result = await Auth.SignInAnonymouslyAsync().AsUniTask();
                return result?.User;;
            }
            catch (Exception e) {
                Debug.LogError($"[Blossom:Firebase:Auth] SignInAnonymouslyAsync failed: {e}");
                return null;
            }
        }

        #endregion

        #region Sign-Out
        
        internal static void SignOut() {
            if (!IsInitialized || Auth == null) return;
            Auth.SignOut();
        }

        #endregion
        
        private static void OnAuthStateChanged(object sender, EventArgs args) {
            if (Auth == null) return;
            if (Auth.CurrentUser == CurrentUser) return;

            FirebaseUser prevUser = CurrentUser;
            FirebaseUser nextUser = Auth.CurrentUser;

            bool signedIn = prevUser != nextUser && nextUser != null && nextUser.IsValid();
            bool signedOut = prevUser != null && (nextUser == null || !nextUser.IsValid());
            
            CurrentUser = nextUser;

            OnUserChanged?.Invoke(CurrentUser);

            if (signedIn) OnSignedIn?.Invoke(CurrentUser);
            if (signedOut) OnSignedOut?.Invoke();
        }
        
#if UNITY_EDITOR
        internal static void SetEmulatorHost(string host) {
            if (string.IsNullOrEmpty(host)) return;
            Environment.SetEnvironmentVariable("FIREBASE_AUTH_EMULATOR_HOST", host);
        }
#endif
        
    }
}

#endif