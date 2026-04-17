namespace Blossom.Monetization.IAP.Internal {
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    using Cysharp.Threading.Tasks;
    using Common;
    
    internal static class IAPSystem {

        #region Properties

        internal static bool IsInitialized { get; private set; }
        internal static bool IsInitializing { get; private set; }
        internal static IAPSettingsSO Settings { get; private set; }

        #endregion

        #region Fields

        private static IAPCatalog _catalog;
        private static IAPPurchaseStorage _storage;
        private static IAPReceiptValidator _validator;
        private static IAPStoreController _controller;

        private static Action _onInitializeCompleted;

        #endregion

        #region Events

        internal static event Action OnInitialized;
        internal static event Action<IAPPurchaseEventArgs> OnPurchaseResult;
        internal static event Action<bool> OnLoadingRequested;
        internal static event Action OnRestoreCompleted;
        internal static event Action OnRestoreFailed;

        #endregion

        #region Initialize

        internal static void Initialize(IAPSettingsSO settings, IIAPPurchaseStorage storage, byte[] googleTangle,
            byte[] appleTangle) {
            if (IsInitialized || IsInitializing) return;
            IsInitializing = true;

            Settings = settings ?? SettingsLoader.Get<IAPSettingsSO>();
            if (Settings == null) {
                IsInitializing = false;
                Debug.LogError("[Blossom:IAP] Initializion failed. SettingsSO not found.");
                return;
            }

            _catalog = new(Settings);
            _storage = new(storage);
            _validator = new(googleTangle, appleTangle);
            
            InitializeStoreAsync().Forget();
        }

        private static async UniTaskVoid InitializeStoreAsync() {
            _controller = new();

            _controller.OnStoreInitialized += HandleControllerInitialized;
            _controller.OnPurchaseSucceeded += HandlePurchaseSucceeded;
            _controller.OnPurchaseFailedByReason += HandlePurchaseFailed;
            _controller.OnPurchaseFailedByDescription += HandlePurchaseFailed;
            _controller.OnRestoreFinished += HandleRestoreFinished;
            _controller.OnLoadingRequested += show => OnLoadingRequested?.Invoke(show);
            
            try {
                await _controller.InitializeAsync(Settings);
            }
            finally {
                IsInitializing = false;
            }
        }

        #endregion

        #region Query

        internal static bool TryGetTagId(string tagName, out IAPTagId tagId) {
            tagId = default;
            return _catalog != null && _catalog.TryGetTagId(tagName, out tagId);
        }
        
        internal static IAPDefinition GetDefinitionByKey(string key) => _catalog?.GetDefinitionByKey(key);
        
        internal static IAPDefinition GetDefinitionById(string productId) => _catalog?.GetDefinitionById(productId);

        internal static Product GetProductByKey(string key) {
            IAPDefinition definition = GetDefinitionByKey(key);
            return definition == null ? null : _controller?.GetProduct(definition.Id);
        }

        internal static IAPProductInfo GetInfo(string key) {
            if (!IsInitialized) {
                Debug.LogError("[Blossom:IAP] IAP is not initialized.");
                return new();
            }
            
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) {
                Debug.LogWarning($"[Blossom:IAP] Product not found. (Key: {key})");
                return new();
            }

            return _controller != null ? _controller.GetProductInfo(definition.Id) : new();
        }

        internal static bool IsSubscription(string key) {
            if (!IsInitialized) {
                Debug.LogError("[Blossom:IAP] IAP is not initialized.");
                return false;
            }

            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null || definition.Type != ProductType.Subscription) return false;

            return _controller != null && _controller.IsSubscribed(definition.Id);
        }

        internal static bool IsPurchased(string key, bool requireValidation = true) {
            if (!IsInitialized) {
                Debug.LogError("[Blossom:IAP] IAP is not initialized.");
                return false;
            }

            // 상품 정보가 존재하지 않으면 false.
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) {
                Debug.LogWarning($"[Blossom:IAP] Product not found. (Key: {key})");
                return false;
            }

            // 소모성은 로컬 저장된 항목 검사.
            if (definition.Type == ProductType.Consumable)
                return _storage != null && _storage.HasEverPurchasedConsumable(key, _catalog);

            Product product = _controller?.GetProduct(definition.Id);
            if (product == null) {
                Debug.LogWarning($"[Blossom:IAP] Product not found. (ID: {definition.Id})");
                return false;
            }

            // 영수증 존재 여부 검사.
            if (!product.hasReceipt) return false;
            
            // 구독형은 구독 상태 체크.
            if (definition.Type == ProductType.Subscription) return IsSubscription(key);
            
            // 검증 필요 시, 검증 체크.
            if (requireValidation)
                return _validator != null && _validator.Validate(product.receipt, product.definition.id);

            return true;
        }

        internal static bool IsPurchasedByTag(IAPTagId tagId, bool requireValidation = true) {
            if (!IsInitialized) {
                Debug.LogError("[Blossom:IAP] IAP is not initialized.");
                return false;
            }

            if (!tagId.IsValid || _catalog == null) return false;

            foreach (IAPDefinition definition in _catalog.GetDefinitionsByTag(tagId)) {
                if (definition == null) continue;
                if (IsPurchased(definition.Key, requireValidation)) return true;
            }

            return false;
        }

        internal static bool IsPurchasedByTag(string tagName, bool requiredValidation = true) {
            return TryGetTagId(tagName, out IAPTagId tagId) && IsPurchasedByTag(tagId, requiredValidation);
        }

        internal static string GetLocalPrice(string key, bool removeCurrencyCode = true) {
            IAPProductInfo info = GetInfo(key);
            if (info == null) {
                Debug.LogWarning($"[Blossom:IAP] Product not found. (Key: {key})");
                return string.Empty;
            }

            string price = $"{info.Price}";
            if (removeCurrencyCode) price = RemoveCurrencySymbols(price);
            return price;
        }

        #endregion

        #region Validation

        private static void ValidateReceiptsAll() {
            if (_controller?.Store?.products == null || _validator == null || !_controller.IsInitialized) return;

            Product[] products = _controller.Store.products.all;
            if (products == null || products.Length <= 0) return;

            foreach (Product product in products) {
                if (product == null || !product.hasReceipt) continue;
                if (product.definition.type == ProductType.Consumable) continue;

                string productId = product.definition.id;
                IAPReceiptValidationResult result = _validator.ValidateDetailed(product.receipt, productId);

                if (!result.IsCanceled) continue;

                string transactionId = !string.IsNullOrWhiteSpace(result.TransactionId)
                    ? result.TransactionId
                    : product.transactionID ?? string.Empty;

                DateTime cancelUtc = result.CancelUtc ?? DateTime.UtcNow;
                _storage?.MarkPurchaseCanceled(productId, transactionId, cancelUtc);
            }
        }

        #endregion

        #region Purchase

        internal static void Purchase(string key) {
            if (!IsInitialized) {
                Debug.LogError("[Blossom:IAP] IAP is not initialized.");
                return;
            }

            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) {
                Debug.LogWarning($"[Blossom:IAP] Product not found. (Key: {key})");
                return;
            }
            
            _controller?.Purchase(definition.Id);
        }

        internal static void Restore() {
            if (!IsInitialized) {
                Debug.LogError("[Blossom:IAP] IAP is not initialized.");
                return;
            }

            _controller?.Restore();
        }

        #endregion

        #region Handle Events

        private static void HandleControllerInitialized() {
            IsInitialized = true;

            ValidateReceiptsAll();
            
            Action callback = _onInitializeCompleted;
            _onInitializeCompleted = null;
            callback?.Invoke();

            OnInitialized?.Invoke();
        }

        private static void HandlePurchaseSucceeded(Product product) {
            if (product == null) return;
            
            _storage?.AddPurchaseInfo(product);

            IAPDefinition definition = GetDefinitionById(product.definition.id);
            if (definition != null) OnPurchaseResult?.Invoke(new(definition, product));
            else Debug.LogWarning($"[Blossom:IAP] Product was not found. (ID: {product.definition.id})");
        }

        private static void HandlePurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            IAPDefinition definition = GetDefinitionById(product?.definition?.id);
            if (definition != null) OnPurchaseResult?.Invoke(new(definition, product, failureReason));
            else Debug.LogWarning($"[Blossom:IAP] Product was not found. (ID: {product?.definition?.id})");
        }

        private static void HandlePurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
            IAPDefinition definition = GetDefinitionById(product?.definition?.id);
            if (definition != null) OnPurchaseResult?.Invoke(new(definition, product, failureDescription));
            else Debug.LogWarning($"[Blossom:IAP] Product was not found. (ID: {product?.definition?.id})");
        }

        private static void HandleRestoreFinished(bool result) {
            if (result) OnRestoreCompleted?.Invoke();
            else OnRestoreFailed?.Invoke();
        }

        #endregion

        #region Utilities

        private static string RemoveCurrencySymbols(string price) {
            if (string.IsNullOrEmpty(price)) return price;

            string[] currencySymbols = { "$", "₩", "€", "£", "¥", "₹", "₽", "₺", "₴", "₿", "¢" };
            price = currencySymbols.Aggregate(price, (current, t) => current.Replace(t, ""));

            return Regex.Replace(price, @"\p{Sc}", "").Trim();
        }

        #endregion
        
    }
}