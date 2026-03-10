using System;
using System.Collections.Generic;
using Blossom.Networking.Online;
using Cysharp.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using UnityEngine.Purchasing.Security;

namespace Blossom.Monetization.IAP {
    internal class IAPStore : IDetailedStoreListener {
        
        #region Const.

        private const string DefaultEnvironmentName = "production";
        
        #endregion

        #region Properties

        public static IStoreController Store { get; private set; }

        public static IExtensionProvider Extensions { get; private set; }

        #endregion

        #region Fields

        private byte[] _googleTangle;
        private byte[] _appleTangle;
        
        private Dictionary<string, bool> _purchaseValidation = new();

        #endregion

        #region Initialize

        public async void Initialize(IAPSettingsSO setting, byte[] googleTangle, byte[] appleTangle) {
            _googleTangle = googleTangle;
            _appleTangle = appleTangle;
            try {
                // 온라인 연결 여부 확인.
                if (await Online.ValidateAsync(OnlineValidationType.All) == false) return;

                // UnityServices 환경 지정 및 초기화.
                await UnityServices
                    .InitializeAsync(new InitializationOptions().SetEnvironmentName(DefaultEnvironmentName))
                    .AsUniTask();

                // Unity IAP 초기화 준비: 모듈 및 빌드 생성.
                StandardPurchasingModule purchasingModule = StandardPurchasingModule.Instance(); // 플랫폼(AOS/IOS)에 맞는 기본 결제 모듈.
                ConfigurationBuilder configureBuilder = ConfigurationBuilder.Instance(purchasingModule); // 상품 등록 빌더.

                // 상품 등록.
                foreach (IAPDefinition definition in setting.Definitions) {
                    if (!string.IsNullOrEmpty(definition.ID)) {
                        configureBuilder.AddProduct(definition.ID, definition.Type);
                    }
                    else Nyo.Error("IAPDefinition's ID is empty.");
                }
                
                UnityPurchasing.Initialize(this, configureBuilder);
                
                // 이후 성공 시 OnInitialized 호출, 실패 시 OnInitializeFailed 호출.
            }
            catch (Exception e) {
                Nyo.Error($"Initialize Error: {e}");
            }
        }

        // Initialize 성공 시 호출됨.
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            Store = controller;
            Extensions = extensions;
            IAP.CallOnInitialized();
        }

        // Initialize 실패 시 호출됨.
        public void OnInitializeFailed(InitializationFailureReason error) {
            Nyo.Error($"Initialize Failed: {error}");
        }

        // Initialize 실패 시 호출됨.
        public void OnInitializeFailed(InitializationFailureReason error, string message) {
            Nyo.Error($"Initialize Failed: {error}, {message}");
        }

        #endregion

        #region Validate

        public bool IsValidated(string productID) {
            return _purchaseValidation.GetValueOrDefault(productID, false);
        }
        
        internal void ValidateAll() {
            if (Store == null) return;
            foreach (Product product in Store.products.all) {
                if (!product.hasReceipt || product.definition.type == ProductType.Consumable) continue;

                string productId = product.definition.id;
                ReceiptValidationResult result = ValidateReceiptDetailed(product.receipt, productId);
                _purchaseValidation[productId] = result.IsValid;
                
                if (result.IsCanceled) {
                    string transactionID = !string.IsNullOrEmpty(result.TransactionId)
                        ? result.TransactionId
                        : product.transactionID ?? string.Empty;
                    DateTime cancelUtc = result.CancelUtc ?? DateTime.UtcNow;
                    IAP.MarkPurchaseCanceled(productId, transactionID, cancelUtc);
                }
            }
        }

        internal bool ValidateReceipt(string receipt, string productID) {
            return ValidateReceiptDetailed(receipt, productID).IsValid;
        }

        private ReceiptValidationResult ValidateReceiptDetailed(string receipt, string productID) {
            try {
                CrossPlatformValidator validator = new(_googleTangle, _appleTangle, Application.identifier);
                IPurchaseReceipt[] result = validator.Validate(receipt);
                // result 안에 같은 productID가 여러 번 있을 수 있음. 가장 최신 영수증을 기준으로 판단.
#if UNITY_ANDROID
                GooglePlayReceipt latestGoogleReceipt = null;
                foreach (IPurchaseReceipt purchaseReceipt in result) {
                    if (purchaseReceipt is not GooglePlayReceipt googlePlayReceipt) continue;
                    if (googlePlayReceipt.productID != productID) continue;
                    if (latestGoogleReceipt == null ||
                        latestGoogleReceipt.purchaseDate < googlePlayReceipt.purchaseDate)
                        latestGoogleReceipt = googlePlayReceipt;
                }

                if (latestGoogleReceipt == null) return new ReceiptValidationResult(false, false, null, string.Empty);

                bool canceled = latestGoogleReceipt.purchaseState != GooglePurchaseState.Purchased;
                bool valid = latestGoogleReceipt.purchaseState == GooglePurchaseState.Purchased;
                DateTime? cancelUtc = canceled ? DateTime.UtcNow : null;
                return new ReceiptValidationResult(valid, canceled, cancelUtc, latestGoogleReceipt.transactionID ?? string.Empty);
#elif UNITY_IOS
                AppleInAppPurchaseReceipt latestAppleReceipt = null;
                foreach (IPurchaseReceipt purchaseReceipt in result) {
                    if (purchaseReceipt is not AppleInAppPurchaseReceipt appleInAppReceipt) continue;
                    if (appleInAppReceipt.productID != productID) continue;
                    if (latestAppleReceipt == null ||
                        latestAppleReceipt.purchaseDate < appleInAppReceipt.purchaseDate)
                        latestAppleReceipt = appleInAppReceipt;
                }
                if (latestAppleReceipt == null) return new ReceiptValidationResult(false, false, null, string.Empty);

                bool canceled = latestAppleReceipt.cancellationDate != DateTime.MinValue;
                DateTime? cancelUtc =
 canceled ? DateTime.SpecifyKind(latestAppleReceipt.cancellationDate, DateTimeKind.Utc) : null;
                bool valid = !canceled;
                return new ReceiptValidationResult(valid, canceled, cancelUtc, latestAppleReceipt.transactionID ?? string.Empty);
#else
                return new ReceiptValidationResult(true, false, null, string.Empty);
#endif
            }
            catch (Exception e) {
                Nyo.Error($"Receipt validation failed: {e}");
                return new ReceiptValidationResult(false, false, null, string.Empty);
            }
        }

        #endregion

        #region Purchase Interface

        // 구매 성공 시 호출.
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
            Product product = purchaseEvent.purchasedProduct;
            
            // 구매 완료 이벤트 요청.
            IAPDefinition definition = IAP.GetDefinitionByID(product.definition.id);
            if (definition != null) IAP.CallOnPurchaseResult(new(definition, product));
            else Nyo.Warning($"Product with the type {product.definition.id} was not found.");

            // 로딩 숨김 요청.
            IAP.CallOnLoadingRequested(false);
            
            // 구매 기록 저장소에 등록.
            IAP.AddPurchaseInfo(product);

            return PurchaseProcessingResult.Complete;
        }

        // 구매 실패 시 호출.
        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            // 구매 실패 이벤트 요청.
            IAPDefinition definition = IAP.GetDefinitionByID(product.definition.id);
            if (definition != null) IAP.CallOnPurchaseResult(new(definition, product, failureReason));
            else Nyo.Warning($"Product with the type {product.definition.id} was not found.");
            
            // 로딩 숨김 요청.
            IAP.CallOnLoadingRequested(false);
            
            Nyo.Log($"Purchase failed: {product.definition.id}, Reason: {failureReason}");
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
            // 구매 실패 이벤트 요청.
            IAPDefinition definition = IAP.GetDefinitionByID(product.definition.id);
            if (definition != null) IAP.CallOnPurchaseResult(new(definition, product, failureDescription));
            else Nyo.Warning($"Product with the type {product.definition.id} was not found.");
            
            // 로딩 숨김 요청.
            IAP.CallOnLoadingRequested(false);
            
            Nyo.Log($"Purchase failed: {product.definition.id}, Reason: {failureDescription.reason}");
        }

        #endregion

        #region Purchase

        public void Purchase(string key) {
            if (!IAP.IsInitialized) {
                Nyo.Error("IAP is not initialized.");
                return;
            }
            
            IAP.CallOnLoadingRequested(true);

            IAPDefinition definition = IAP.GetDefinitionByKey(key);
            if (definition != null) Store.InitiatePurchase(definition.ID);
        }

        public void Restore() {
            if (!IAP.IsInitialized) {
                Nyo.Error("IAP is not initialized.");
                return;
            }

            IAP.CallOnLoadingRequested(true);

            Extensions.GetExtension<IAppleExtensions>().RestoreTransactions((result, message) => {
                if (result) IAP.CallOnRestoreCompleted();
                else IAP.CallOnRestoreFailed();
                IAP.CallOnLoadingRequested(false);
            });

        }

        public IAPProductInfo GetInfo(string key) {
            if (!IAP.IsInitialized) {
                Nyo.Error("IAP is not initialized.");
                return null;
            }

            IAPDefinition definition = IAP.GetDefinitionByKey(key);
            return definition != null ? new IAPProductInfo(Store.products.WithID(definition.ID)) : null;
        }
        
        public bool IsSubscribed(string key) {
            IAPDefinition definition = IAP.GetDefinitionByKey(key);
            if (definition == null) return false;
            
            Product product = Store.products.WithID(definition.ID);
            if (product?.receipt == null) return false;

            return new SubscriptionManager(product, null).getSubscriptionInfo().isSubscribed() == Result.True;
        }

        #endregion

        private readonly struct ReceiptValidationResult {
            public readonly bool IsValid;
            public readonly bool IsCanceled;
            public readonly DateTime? CancelUtc;
            public readonly string TransactionId;

            public ReceiptValidationResult(bool isValid, bool isCanceled, DateTime? cancelUtc, string transactionId) {
                IsValid = isValid;
                IsCanceled = isCanceled;
                CancelUtc = cancelUtc;
                TransactionId = transactionId ?? string.Empty;
            }
        }
        
    }
}