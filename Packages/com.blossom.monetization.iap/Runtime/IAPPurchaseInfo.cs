using System;
using UnityEngine;

namespace Blossom.Monetization.IAP {
    [Serializable]
    public sealed class IAPPurchaseInfo {

        #region Properties

        public string ProductId => productId;
        public string TransactionId => transactionId;
        public long PurchaseUtcTicks => purchaseUtcTicks;
        public long CancelUtcTicks => cancelUtcTicks;

        public bool IsCanceled => cancelUtcTicks > 0;
        public DateTime PurchaseDate => new(purchaseUtcTicks, DateTimeKind.Utc);
        public DateTime CancelDate => cancelUtcTicks > 0 ? new(cancelUtcTicks, DateTimeKind.Utc) : DateTime.MinValue;

        #endregion
        
        #region Fields

        [SerializeField] private string productId;
        [SerializeField] private string transactionId;
        [SerializeField] private long purchaseUtcTicks;
        [SerializeField] private long cancelUtcTicks;

        #endregion

        public IAPPurchaseInfo(string productId, string transactionId, DateTime purchaseDate) {
            this.productId = productId ?? string.Empty;
            this.transactionId = transactionId ?? string.Empty;
            this.purchaseUtcTicks = purchaseDate.Kind == DateTimeKind.Utc
                ? purchaseDate.Ticks
                : purchaseDate.ToUniversalTime().Ticks;
            this.cancelUtcTicks = 0;
        }

        public IAPPurchaseInfo MarkCanceled(DateTime cancelDate) {
            cancelUtcTicks = cancelDate.Kind == DateTimeKind.Utc
                ? cancelDate.Ticks
                : cancelDate.ToUniversalTime().Ticks;
            return this;
        }

    }
}