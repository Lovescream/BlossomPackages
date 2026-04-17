namespace Blossom.Monetization.IAP.Internal {
    using System;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using Cysharp.Threading.Tasks;
    using Networking.Online;
    using UnityEngine;

    internal sealed class IAPStoreController : IDetailedStoreListener {
        
        #region Const.

        private const string DefaultEnvironmentName = "production";
        
        #endregion

        #region Properties

        internal bool IsInitialized => Store != null && Extensions != null;

        internal IStoreController Store { get; private set; }
        internal IExtensionProvider Extensions { get; private set; }

        #endregion

        #region Events

        internal event Action OnStoreInitialized;
        internal event Action<Product> OnPurchaseSucceeded;
        internal event Action<Product, PurchaseFailureReason> OnPurchaseFailedByReason;
        internal event Action<Product, PurchaseFailureDescription> OnPurchaseFailedByDescription;
        internal event Action<bool> OnRestoreFinished;
        internal event Action<bool> OnLoadingRequested;
        internal event Action<InitializationFailureReason, string> OnStoreInitializeFailed;

        #endregion

        #region Initialize

        internal async UniTask InitializeAsync(IAPSettingsSO settings) {
            if (settings == null) {
                OnStoreInitializeFailed?.Invoke(InitializationFailureReason.PurchasingUnavailable, "IAPSettingsSO is null.");
                return;
            }

            try {
                // 온라인 연결 여부 확인.
                bool isOnline = await Online.ValidateAsync(OnlineValidationType.All);
                if (!isOnline) {
                    OnStoreInitializeFailed?.Invoke(InitializationFailureReason.PurchasingUnavailable,
                        "Online validation failed.");
                    return;
                }

                // UnityServices 환경 지정 및 초기화.
                await UnityServices
                    .InitializeAsync(new InitializationOptions().SetEnvironmentName(DefaultEnvironmentName))
                    .AsUniTask();
                
                // Unity IAP 초기화 준비: 모듈 및 빌드 생성.
                StandardPurchasingModule purchasingModule = StandardPurchasingModule.Instance();
                ConfigurationBuilder configureBuilder = ConfigurationBuilder.Instance(purchasingModule);
                
                // 상품 등록.
                IAPDefinition[] definitions = settings.Definitions;
                if (definitions != null) {
                    foreach (IAPDefinition definition in definitions) {
                        if (definition == null) continue;
                        if (string.IsNullOrWhiteSpace(definition.Id)) {
                            Debug.LogError($"[Blossom:IAP] IAPDefinition's ID is empty. (Key: {definition.Key}");
                            continue;
                        }

                        configureBuilder.AddProduct(definition.Id, definition.Type);
                    }
                }

                UnityPurchasing.Initialize(this, configureBuilder);
                
                // 이후 성공 시 OnInitialized 호출, 실패 시 OnInitializeFailed 호출.
            }
            catch (Exception e) {
                Debug.LogError($"[Blossom:IAP] Initialize Error: {e}");
                OnStoreInitializeFailed?.Invoke(InitializationFailureReason.PurchasingUnavailable, e.Message);
            }
        }

        // Initialize 성공 시 호출됨.
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            Store = controller;
            Extensions = extensions;
            OnStoreInitialized?.Invoke();
        }
        
        // Initialize 실패 시 호출됨.
        public void OnInitializeFailed(InitializationFailureReason error) {
            Debug.LogError($"[Blossom:IAP] Initialize Failed: {error}");
            OnStoreInitializeFailed?.Invoke(error, string.Empty);
        }

        // Initialize 실패 시 호출됨.
        public void OnInitializeFailed(InitializationFailureReason error, string message) {
            Debug.LogError($"[Blossom:IAP] Initialize Failed: {error}, {message}");
            OnStoreInitializeFailed?.Invoke(error, message);
        }
        
        #endregion

        #region Query

        internal Product GetProduct(string productId) {
            if (!IsInitialized) return null;
            if (string.IsNullOrEmpty(productId)) return null;
            return Store.products.WithID(productId);
        }

        internal IAPProductInfo GetProductInfo(string productId) {
            Product product = GetProduct(productId);
            return product != null ? new(product) : new();
        }

        internal bool IsSubscribed(string productId) {
            Product product = GetProduct(productId);
            if (product?.receipt == null) return false;

            return new SubscriptionManager(product, null).getSubscriptionInfo().isSubscribed() == Result.True;
        }

        #endregion

        #region Purchase

        internal void Purchase(string productId) {
            if (!IsInitialized) {
                Debug.LogError($"[Blossom:IAP] IAPStoreController is not initialized.");
                return;
            }

            Product product = GetProduct(productId);
            if (product == null) {
                Debug.LogWarning($"[Blossom:IAP] Product not found. (ID: {productId}");
                return;
            }

            OnLoadingRequested?.Invoke(true);
            Store.InitiatePurchase(product);
        }

        internal void Restore() {
            if (!IsInitialized) {
                Debug.LogError($"[Blossom:IAP] IAPStoreController is not initialized.");
                return;
            }

            OnLoadingRequested?.Invoke(true);

#if UNITY_IOS
            Extensions.GetExtension<IAppleExtensions>().RestoreTransactions((result, message) => {
                OnLoadingRequested?.Invoke(false);
                OnRestoreFinished?.Invoke(result);
            });
#else
            OnLoadingRequested?.Invoke(false);
            OnRestoreFinished?.Invoke(false);
#endif
        }

        #endregion

        #region Listener

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
            Product product = purchaseEvent?.purchasedProduct;
            OnLoadingRequested?.Invoke(false);
            
            if (product != null) OnPurchaseSucceeded?.Invoke(product);
            
            return PurchaseProcessingResult.Complete;
        }

        // 구매 실패 시 호출.
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            OnLoadingRequested?.Invoke(false);
            OnPurchaseFailedByReason?.Invoke(product, failureReason);
            Debug.Log($"[Blossom:IAP] Purchase Failed: (ID: {product?.definition.id}) {failureReason}");
        }

        // 구매 실패 시 호출.
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
            OnLoadingRequested?.Invoke(false);
            OnPurchaseFailedByDescription?.Invoke(product, failureDescription);
            Debug.Log($"[Blossom:IAP] Purchase Failed: (ID: {product?.definition.id}) {failureDescription.reason}");
        }

        #endregion
        
    }
}