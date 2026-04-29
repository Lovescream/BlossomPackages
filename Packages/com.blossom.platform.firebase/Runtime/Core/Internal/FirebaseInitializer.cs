#if SDK_FIREBASE

using Firebase;

namespace Blossom.Platform.Firebase.Internal {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    
    internal static class FirebaseInitializer {

        #region Const.

        private const float InitializeTimeout = 15f;

        #endregion

        #region Properties

        internal static bool IsInitialized { get; private set; }
        internal static bool IsInitializing { get; private set; }
        internal static FirebaseApp App { get; private set; }

        #endregion

        #region Fields

        private static readonly List<IFirebaseModule> Modules = new();

        internal static event Action OnInitialized;
        private static Action _cbOnInitializeCompleted;
        private static UniTaskCompletionSource<bool> _initializeTaskSource;

        #endregion

        #region Initialize

        internal static void Initialize(Action onComplete = null) {
            if (IsInitialized) {
                onComplete?.Invoke();
                return;
            }

            if (onComplete != null) _cbOnInitializeCompleted += onComplete;

            if (IsInitializing) return;

            _initializeTaskSource = new();
            InitializeAsyncInternal().Forget();
        }

        internal static UniTask<bool> InitializeAsync() {
            if (IsInitialized) return UniTask.FromResult(true);

            Initialize();
            return _initializeTaskSource?.Task ?? UniTask.FromResult(false);
        }
        
        private static async UniTaskVoid InitializeAsyncInternal() {
            IsInitializing = true;

            try {
                DependencyStatus dependencyStatus;
                try {
                    dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync()
                        .AsUniTask().Timeout(TimeSpan.FromSeconds(InitializeTimeout));
                }
                catch (TimeoutException) {
                    Debug.LogError("[Blossom:Firebase] Dependency check time out.");
                    FailInitialize();
                    return;
                }
                catch (Exception e) {
                    Debug.LogError($"[Blossom:Firebase] Dependency check failed: {e}");
                    FailInitialize();
                    return;
                }

                if (dependencyStatus != DependencyStatus.Available) {
                    Debug.LogError($"[Blossom:Firebase] Dependency unavailable: {dependencyStatus}");
                    FailInitialize();
                    return;
                }

                App = FirebaseApp.DefaultInstance;
                if (App == null) {
                    Debug.LogError($"[Blossom:Firebase] FirebaseApp.DefaultInstance is null.");
                    FailInitialize();
                    return;
                }

                await InitializeModulesAsync(App);

                IsInitialized = true;

                Action callback = _cbOnInitializeCompleted;
                _cbOnInitializeCompleted = null;
                callback?.Invoke();
                OnInitialized?.Invoke();

                _initializeTaskSource?.TrySetResult(true);
            }
            finally {
                IsInitializing = false;
            }
        }

        private static async UniTask InitializeModulesAsync(FirebaseApp app) {
            foreach (IFirebaseModule module in Modules) {
                if (module == null) continue;
                try {
                    await module.InitializeAsync(app);
                }
                catch (Exception e) {
                    Debug.LogError($"[Blossom:Firebase] Error initializing module (Name: {module.ModuleName}): {e}");
                    throw;
                }
            }
        }

        #endregion

        // 모듈 등록: 중복 등록 방지.
        internal static void RegisterModule(IFirebaseModule module) {
            if (module == null) return;
            if (IsInitializing || IsInitialized) {
                Debug.LogWarning("[Blossom:Firebase] RegisterModule() must be called before Firebase initialization.");
                return;
            }
            Type moduleType = module.GetType();
            foreach (IFirebaseModule registered in Modules) {
                if (registered == null) continue;
                if (registered.GetType() == moduleType) return;
            }

            Modules.Add(module);
        }

        private static void FailInitialize() {
            _cbOnInitializeCompleted = null;
            _initializeTaskSource?.TrySetResult(false);
        }

    }
}

#endif