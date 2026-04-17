namespace Blossom.Monetization.IAP.Internal {
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using UnityEngine.Purchasing;

    internal sealed class IAPPurchaseStorage {

        #region Fields

        private IIAPPurchaseStorage _storage;
        private readonly Dictionary<string, List<IAPPurchaseInfo>> _purchaseInfos = new(StringComparer.Ordinal);

        #endregion
        
        #region Constructor

        internal IAPPurchaseStorage(IIAPPurchaseStorage storage) {
            Initialize(storage);
        }

        internal void Initialize(IIAPPurchaseStorage storage) {
            _storage = storage;
            Reload();
        }

        // 연결된 저장소로부터 구매 기록을 다시 로드하여 런타임 캐시 갱신.
        // 저장소가 연결되어 있지 않으면 캐시는 비워진 상태로 유지됨.
        internal void Reload() {
            _purchaseInfos.Clear();

            if (_storage == null) return;
            IReadOnlyList<IAPPurchaseInfo> infos = _storage.LoadAll();
            if (infos == null || infos.Count <= 0) return;

            foreach (IAPPurchaseInfo info in infos) {
                if (info == null || string.IsNullOrWhiteSpace(info.ProductId)) continue;

                if (!_purchaseInfos.TryGetValue(info.ProductId, out List<IAPPurchaseInfo> list)) {
                    list = new();
                    _purchaseInfos.Add(info.ProductId, list);
                }

                list.Add(info);
            }

            // 구매 기록을 기준으로 오름차순 정렬.
            foreach (List<IAPPurchaseInfo> list in _purchaseInfos.Values) {
                list.Sort((a, b) => a.PurchaseUtcTicks.CompareTo(b.PurchaseUtcTicks));
            }
        }

        #endregion

        #region Query

        // ProductId 기반 상품 구매 횟수 조회.
        internal int GetPurchaseCountByProductId(string productId, bool includeCanceled = false) {
            if (string.IsNullOrWhiteSpace(productId)) return 0;
            if (!_purchaseInfos.TryGetValue(productId, out List<IAPPurchaseInfo> list) || list == null) return 0;

            return list.Where(info => info != null).Count(info => includeCanceled || !info.IsCanceled);
        }

        // 소모성 상품을 구매한 적이 있는지 조회.
        internal bool HasEverPurchasedConsumable(string productKey, IAPCatalog catalog) {
            if (_storage == null || catalog == null) return false;

            IAPDefinition definition = catalog.GetDefinitionByKey(productKey);
            if (definition == null || definition.Type != ProductType.Consumable) return false;

            return GetPurchaseCountByProductId(definition.Id) > 0;
        }

        #endregion

        #region Update

        // 구매 정보 추가.
        internal void AddPurchaseInfo(Product product) {
            if (_storage == null || product == null) return;

            string productId = product.definition.id ?? string.Empty;
            if (string.IsNullOrWhiteSpace(productId)) return;

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

        internal void MarkPurchaseCanceled(string productId, string transactionId, DateTime cancelUtc) {
            if (_storage == null || string.IsNullOrWhiteSpace(productId)) return;

            long cancelUtcTicks = cancelUtc.Kind == DateTimeKind.Utc
                ? cancelUtc.Ticks
                : cancelUtc.ToUniversalTime().Ticks;
            
            MarkPurchaseCanceledInCache(productId, transactionId, cancelUtcTicks);
            _storage.MarkCanceled(productId, transactionId, cancelUtcTicks);
            _storage.Save();
        }

        private void MarkPurchaseCanceledInCache(string productId, string transactionId, long cancelUtcTicks) {
            if (!_purchaseInfos.TryGetValue(productId, out List<IAPPurchaseInfo> list) || list == null) return;

            // transactionId로 찾기.
            if (!string.IsNullOrWhiteSpace(transactionId)) {
                for (int i = list.Count - 1; i >= 0; i--) {
                    IAPPurchaseInfo info = list[i];
                    if (info == null) continue;
                    if (info.IsCanceled) continue;
                    if (!string.Equals(info.TransactionId, transactionId, StringComparison.Ordinal)) continue;

                    info.MarkCanceled(new DateTime(cancelUtcTicks, DateTimeKind.Utc));
                    return;
                }
            }

            // fallback: 가장 최근 미취소 기록을 취소 처리.
            for (int i = list.Count - 1; i >= 0; i--) {
                IAPPurchaseInfo info = list[i];
                if (info == null) continue;
                if (info.IsCanceled) continue;

                info.MarkCanceled(new DateTime(cancelUtcTicks, DateTimeKind.Utc));
                return;
            }
        }

        #endregion

    }
}