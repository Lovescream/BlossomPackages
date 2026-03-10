using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Blossom.Common;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Blossom.Monetization.IAP {
    public static class IAP {
        
        #region Properties

        /// <summary>
        /// IAP 시스템이 초기화되었는지 여부.
        /// </summary>
        public static bool IsInitialized { get; private set; }

        #endregion
        
        #region Fields

        private static IAPSettingsSO _settings;
        
        private static IAPStore _store;
        private static IIAPPurchaseStorage _storage;
        private static Dictionary<string, IAPDefinition> _definitions;
        private static readonly Dictionary<string, List<IAPPurchaseInfo>> _purchaseInfos = new();

        public static event Action OnInitialized;
        public static event Action<IAPPurchaseEventArgs> OnPurchaseResult;
        public static event Action<bool> OnLoadingRequested;
        public static event Action OnRestoreCompleted;
        public static event Action OnRestoreFailed;

        #endregion

        #region Initialize

        public static void Initialize(IIAPPurchaseStorage storage, byte[] googleTangle, byte[] appleTangle) {
            Initialize(setting: null, storage, googleTangle, appleTangle);
        }

        public static void Initialize(IAPSettingsSO setting, IIAPPurchaseStorage storage, byte[] googleTangle, byte[] appleTangle) {
            if (IsInitialized) {
                Nyo.Error("IAP is already initialized.");
                return;
            }

            _settings = setting ?? SettingsLoader.Get<IAPSettingsSO>();
            if (_settings == null) {
                Debug.LogError($"[Blossom:IAP] Initialize(): SettingsSO not found.");
                return;
            }
            
            _definitions = new();
            IAPDefinition[] products = _settings.Definitions;
            if (products != null && products.Length > 0) {
                foreach (IAPDefinition product in products) {
                    if (!_definitions.TryAdd(product.Key, product))
                        Nyo.Error($"Product({product.Key}) has duplicates in the list.");
                }
            }

            _store = new();
            _store.Initialize(_settings, googleTangle, appleTangle);
            
            SetPurchaseStorage(storage);
        }

        private static void SetPurchaseStorage(IIAPPurchaseStorage storage) {
            _storage = storage;
            ReloadPurchaseHistory();
        }

        #endregion

        #region Get / Validate

        /// <summary>
        /// 태그 이름 기반 IAPTagId 조회.
        /// </summary>
        /// <param name="tagName">조회할 태그 이름</param>
        /// <param name="tagId">조회 성공 시 대응되는 IAPTagId 할당.</param>
        /// <returns>태그 이름이 존재하면 true, 그렇지 않으면 false.</returns>
        public static bool TryGetTagId(string tagName, out IAPTagId tagId) {
            tagId = default;
            return _settings != null && _settings.TryGetTagId(tagName, out tagId);
        }

        /// <summary>
        /// Key 기반 상품 정의 조회.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>Key에 대응하는 상품 정의. 존재하지 않으면 null.</returns>
        public static IAPDefinition GetDefinitionByKey(string key) {
            return _definitions.GetValueOrDefault(key);
        }
        
        /// <summary>
        /// ProductID 기반 정의 조회.
        /// </summary>
        /// <param name="productID">상품 스토어 ID</param>
        /// <returns>스토어 ID에 대응하는 상품 정의. 존재하지 않으면 null.</returns>
        public static IAPDefinition GetDefinitionByID(string productID) {
            return _definitions.Values.FirstOrDefault(p => p.ID == productID);
        }

        /// <summary>
        /// Key 기반 Product 객체 조회.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>Key에 대응하는 Product 객체. 존재하지 않으면 null.</returns>
        public static Product GetProductByKey(string key) {
            IAPDefinition definition = GetDefinitionByKey(key);
            return definition != null ? IAPStore.Store?.products.WithID(definition.ID) : null;
        }

        /// <summary>
        /// Key 기반 상품 메타 정보 조회. 가격, 통화코드, 구매 여부 등이 포함되어 있음.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>Key에 대응하는 상품 메타 정보. 존재하지 않으면 기본값 반환.</returns>
        public static IAPProductInfo GetInfo(string key) {
            if (!IsInitialized) return new();
            IAPProductInfo info = _store.GetInfo(key);
            if (info == null) Nyo.Warning($"Product not found. (Key: {key})");
            return info;
        }

        /// <summary>
        /// 상품의 구독 여부 확인.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>구독 중이면 true, 아니면 false. 초기화되지 않았다면 false.</returns>
        public static bool IsSubscription(string key) {
            if (!IsInitialized) return false;
            return _store.IsSubscribed(key);
        }

        /// <summary>
        /// 로컬 가격 문자열 반환.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <param name="removeCurrencyCode">true이면 통화 기호 제거, false이면 통화 기호 포함.</param>
        /// <returns>로컬라이즈된 가격 문자열. 상품이 없으면 빈 값.</returns>
        public static string GetLocalPrice(string key, bool removeCurrencyCode = true) {
            IAPProductInfo info = GetInfo(key);
            if (info == null) {
                Nyo.Warning($"Product not found. (Key: {key})");
                return string.Empty;
            }

            string price = $"{info.Price}";
            if (removeCurrencyCode) price = RemoveCurrencySymbols(price);
            return price;
        }

        /// <summary>
        /// 모든 비소모성 상품의 영수증을 검증.
        /// </summary>
        public static void ValidateReceiptAll() {
            if (!IsInitialized) {
                Nyo.Warning("IAP is not initialized.");
                return;
            }
            
            _store.ValidateAll();
        }

        /// <summary>
        /// Key 기반 상품 영수증 검증.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>검증 성공 시 true, 실패(초기화되지 않음, 소모성 상품, 영수증 없음, 검증 실패) 시 false.</returns>
        public static bool ValidateReceipt(string key) {
            if (!IsInitialized) {
                Nyo.Warning("IAP is not initialized.");
                return false;
            }

            IAPProductInfo info = GetInfo(key);
            
            // 소모성 상품의 영수증 검증은 건너뜀: 할 필요가 없어서...? 필요 시 추후 구현.
            if (info.ProductType == ProductType.Consumable) return false;
            
            if (!info.Product.hasReceipt) return false;

            return _store.ValidateReceipt(info.Product.receipt, info.Product.definition.id);
        }

        /// <summary>
        /// 해당 상품이 이전에 검증 성공 상태인지 확인.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>검증 완료 상태면 true, 아니면 false.</returns>
        public static bool IsValidatedReceipt(string key) {
            if (!IsInitialized) {
                Nyo.Warning("IAP is not initialized.");
                return false;
            }
            
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) {
                Nyo.Warning($"Product not found. (Key: {key})");
                return false;
            }

            return _store.IsValidated(definition.ID);
        }

        /// <summary>
        /// 상품을 구매했는지 검사.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <param name="requireValidation">영수증 검증 여부. true이면 '구매하고 보유 중인지' 여부를, false이면 '구매한 적이 있는지' 여부를 검사하게 됨.</param>
        /// <returns></returns>
        public static bool IsPurchased(string key, bool requireValidation = true) {
            if (!IsInitialized) return false;
            
            // 상품 정보가 존재하지 않으면 false.
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) return false;
            
            // 소모성은 로컬 저장된 항목 검사.
            if (definition.Type == ProductType.Consumable) return HasEverPurchasedConsumable(key);
            
            Product product = IAPStore.Store?.products.WithID(definition.ID);
            if (product == null) return false;
            
            // 영수증 존재 여부 검사.
            if (!product.hasReceipt) return false;
            
            // 구독형은 구독 상태 체크.
            if (definition.Type == ProductType.Subscription) return IsSubscription(key);
            
            // 검증 필요 시.
            if (requireValidation) return _store.ValidateReceipt(product.receipt, product.definition.id);

            return true;
        }

        /// <summary>
        /// 태그가 포함된 상품을 구매했는지 검사.
        /// </summary>
        /// <param name="tagId">상품 Tag</param>
        /// <param name="requireValidation">영수증 검증 여부. true이면 '구매하고 보유 중인지' 여부를, false이면 '구매한 적이 있는지' 여부를 검사하게 됨.</param>
        /// <returns></returns>
        public static bool IsPurchasedByTag(IAPTagId tagId, bool requireValidation = true) {
            if (!IsInitialized || !tagId.IsValid) return false;

            foreach (IAPDefinition definition in _definitions.Values) {
                if (!definition.Tags.Contains(tagId)) continue;
                if (IsPurchased(definition.Key, requireValidation)) return true;
            }

            return false;
        }

        /// <summary>
        /// 태그가 포함된 상품을 구매했는지 검사.
        /// </summary>
        /// <param name="tagName">상품 Tag</param>
        /// <param name="requireValidation">영수증 검증 여부. true이면 '구매하고 보유 중인지' 여부를, false이면 '구매한 적이 있는지' 여부를 검사하게 됨.</param>
        /// <returns></returns>
        public static bool IsPurchasedByTag(string tagName, bool requireValidation = true) {
            return TryGetTagId(tagName, out IAPTagId tagId) && IsPurchasedByTag(tagId, requireValidation);
        }
        
        #endregion

        #region Purchase

        /// <summary>
        /// 구매 복원 요청.
        /// </summary>
        public static void Restore() {
            if (!IsInitialized) return;
            _store?.Restore();
        }

        /// <summary>
        /// 특정 상품의 구매 요청.
        /// </summary>
        /// <param name="key">상품 Key</param>
        public static void Purchase(string key) {
            if (!IsInitialized) {
                Nyo.Error("IAP is not initialized.");
                return;
            }
            _store?.Purchase(key);
        }

        #endregion

        #region Storage

        // 연결된 저장소로부터 구매 기록을 다시 로드하여 런타임 캐시 갱신.
        // 저장소가 연결되어 있지 않으면 캐시는 비워진 상태로 유지됨.
        private static void ReloadPurchaseHistory() {
            _purchaseInfos.Clear();
            IReadOnlyList<IAPPurchaseInfo> all = _storage?.LoadAll();
            if (all == null) return;

            foreach (IAPPurchaseInfo info in all) {
                if (string.IsNullOrEmpty(info.ProductId)) continue;
                if (!_purchaseInfos.TryGetValue(info.ProductId, out List<IAPPurchaseInfo> list)) {
                    list = new();
                    _purchaseInfos[info.ProductId] = list;
                }

                list.Add(info);
            }

            // 구매 기록을 기준으로 오름차순 정렬.
            foreach (List<IAPPurchaseInfo> list in _purchaseInfos.Values) {
                list.Sort((a, b) => a.PurchaseUtcTicks.CompareTo(b.PurchaseUtcTicks));
            }
        }

        /// <summary>
        /// 상품 스토어 ID를 기준으로 구매 기록 목록 조회.
        /// </summary>
        /// <param name="productId">상품 스토어 ID</param>
        /// <returns>해당 상품 스토어 ID의 구매 기록 목록. 캐시에 없거나 ID가 유효하지 않으면 빈 배열 반환.</returns>
        public static IReadOnlyList<IAPPurchaseInfo> GetPurchaseInfosByID(string productId) {
            if (string.IsNullOrEmpty(productId)) return Array.Empty<IAPPurchaseInfo>();
            return _purchaseInfos.TryGetValue(productId, out List<IAPPurchaseInfo> list)
                ? list
                : Array.Empty<IAPPurchaseInfo>();
        }

        /// <summary>
        /// 상품 Key를 기준으로 구매 기록 목록 조회.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <returns>해당 상품 Key의 구매 기록 목록. 캐시에 없거나 Key가 유효하지 않으면 빈 배열 반환.</returns>
        public static IReadOnlyList<IAPPurchaseInfo> GetPurchaseInfosByKey(string key) {
            if (string.IsNullOrEmpty(key)) return Array.Empty<IAPPurchaseInfo>();
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) return Array.Empty<IAPPurchaseInfo>();
            return GetPurchaseInfosByID(definition.ID);
        }

        /// <summary>
        /// 저장소에 기록된 모든 상품의 구매 횟수.
        /// </summary>
        /// <param name="includeCanceled">true면 취소된 구매 기록까지 포함하며, false면 취소되지 않은 구매 기록만 카운트.</param>
        /// <returns>모든 상품의 구매 횟수</returns>
        public static int GetPurchaseCount(bool includeCanceled = false) {
            if (_purchaseInfos.Count <= 0) return 0;

            int count = 0;
            foreach (List<IAPPurchaseInfo> list in _purchaseInfos.Values) {
                if (includeCanceled) count += list.Count;
                else count += list.Count(t => !t.IsCanceled);
            }

            return count;
        }

        /// <summary>
        /// 특정 스토어 ID의 상품의 구매 횟수.
        /// </summary>
        /// <param name="productId">상품 스토어 ID</param>
        /// <param name="includeCanceled">true면 취소된 구매 기록까지 포함하며, false면 취소되지 않은 구매 기록만 카운트.</param>
        /// <returns>상품의 구매 횟수</returns>
        public static int GetPurchaseCountByID(string productId, bool includeCanceled = false) {
            IReadOnlyList<IAPPurchaseInfo> list = GetPurchaseInfosByID(productId);
            return includeCanceled ? list.Count : list.Count(t => !t.IsCanceled);
        }

        /// <summary>
        /// 특정 Key 상품의 구매 횟수.
        /// </summary>
        /// <param name="key">상품 Key</param>
        /// <param name="includeCanceled">true면 취소된 구매 기록까지 포함하며, false면 취소되지 않은 구매 기록만 카운트.</param>
        /// <returns>상품의 구매 횟수</returns>
        public static int GetPurchaseCountByKey(string key, bool includeCanceled = false) {
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null) return 0;
            return GetPurchaseCountByID(definition.ID, includeCanceled);
        }

        private static bool HasEverPurchasedConsumable(string key) {
            if (!IsInitialized) return false;
            if (_storage == null) return false;
            IAPDefinition definition = GetDefinitionByKey(key);
            if (definition == null || definition.Type != ProductType.Consumable) return false;
            return GetPurchaseCountByID(definition.ID, includeCanceled: false) > 0;
        }

        internal static void AddPurchaseInfo(Product product) {
            if (_storage == null || product == null) return;
            string productId = product.definition.id;
            if (string.IsNullOrEmpty(productId)) return;
            string transactionId = product.transactionID ?? string.Empty;

            IAPPurchaseInfo info = new(productId, transactionId, DateTime.UtcNow);
            if (!_purchaseInfos.TryGetValue(productId, out List<IAPPurchaseInfo> list)) {
                list = new();
                _purchaseInfos[productId] = list;
            }

            list.Add(info);
            list.Sort((a, b) => a.PurchaseUtcTicks.CompareTo(b.PurchaseUtcTicks));

            _storage.Add(info);
            _storage.Save();
        }

        internal static void MarkPurchaseCanceled(string productId, string transactionId, DateTime cancelUtc) {
            if (_storage == null || string.IsNullOrEmpty(productId)) return;
            long cancelTicks = cancelUtc.Kind == DateTimeKind.Utc ? cancelUtc.Ticks : cancelUtc.ToUniversalTime().Ticks;
            MarkPurchaseCanceledInCache(productId, transactionId, cancelTicks);
            _storage.MarkCanceled(productId, transactionId ?? string.Empty, cancelTicks);
            _storage.Save();
        }

        private static void MarkPurchaseCanceledInCache(string productId, string transactionId, long cancelUtcTicks) {
            if (!_purchaseInfos.TryGetValue(productId, out List<IAPPurchaseInfo> list) || list == null) return;

            // transactionId로 찾기.
            if (!string.IsNullOrEmpty(transactionId)) {
                for (int i = list.Count - 1; i >= 0; i--) {
                    IAPPurchaseInfo info = list[i];
                    if (info.IsCanceled) continue;
                    if (info.TransactionId != transactionId) continue;
                    info.MarkCanceled(new DateTime(cancelUtcTicks, DateTimeKind.Utc));
                    return;
                }
            }
            
            // fallback: 가장 최근 미취소 기록을 취소 처리.
            for (int i = list.Count - 1; i >= 0; i--) {
                IAPPurchaseInfo info = list[i];
                if (info.IsCanceled) continue;
                info.MarkCanceled(new DateTime(cancelUtcTicks, DateTimeKind.Utc));
                return;
            }
        }

        #endregion
        
        #region Events

        internal static void CallOnInitialized() {
            IsInitialized = true;
            OnInitialized?.Invoke();
            
            ValidateReceiptAll();
        }

        internal static void CallOnPurchaseResult(IAPPurchaseEventArgs purchaseEvent) {
            OnPurchaseResult?.Invoke(purchaseEvent);
        }

        internal static void CallOnLoadingRequested(bool show) {
            OnLoadingRequested?.Invoke(show);
        }

        internal static void CallOnRestoreCompleted() {
            OnRestoreCompleted?.Invoke();
        }

        internal static void CallOnRestoreFailed() {
            OnRestoreFailed?.Invoke();
        }

        #endregion

        private static string RemoveCurrencySymbols(string price) {
            if (string.IsNullOrEmpty(price)) return price;

            string[] currencySymbols = { "$", "₩", "€", "£", "¥", "₹", "₽", "₺", "₴", "₿", "¢" };
            foreach (string symbol in currencySymbols) price = price.Replace(symbol, "");
            return Regex.Replace(price, @"\p{Sc}", "").Trim();
        }

    }

    public enum IAPPurchaseStatus {
        Succeeded,
        Failed
    }

}
