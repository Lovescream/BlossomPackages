namespace Blossom.Monetization.IAP {
    using System;
    using UnityEngine;
    
    /// <summary>
    /// 단일 구매 기록 정보.
    /// <para/>
    /// 상품 ID, 거래 ID, 구매 시각, 취소 시각을 보관한다.
    /// </summary>
    [Serializable]
    public sealed class IAPPurchaseInfo {

        #region Properties

        /// <summary>
        /// 구매된 상품의 Product ID.
        /// </summary>
        public string ProductId => productId;
        
        /// <summary>
        /// 거래 ID.
        /// </summary>
        public string TransactionId => transactionId;

        /// <summary>
        /// 구매 시각 (UTC ticks).
        /// </summary>
        public long PurchaseUtcTicks => purchaseUtcTicks;
        
        /// <summary>
        /// 취소 시각 (UTC ticks).
        /// </summary>
        public long CancelUtcTicks => cancelUtcTicks;
        
        /// <summary>
        /// 취소된 구매 기록인지 여부.
        /// </summary>
        public bool IsCanceled => cancelUtcTicks > 0;

        /// <summary>
        /// 구매 시각 (UTC).
        /// </summary>
        public DateTime PurchaseDate => new(purchaseUtcTicks, DateTimeKind.Utc);
        
        /// <summary>
        /// 취소 시각 (UTC). 취소되지 않았다면 <see cref="DateTime.MinValue"/>.
        /// </summary>
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
        
        /// <summary>
        /// 이 구매 기록을 취소 상태로 변경한다.
        /// </summary>
        /// <param name="cancelDate">취소 시각</param>
        /// <returns>자기 자신</returns>
        public IAPPurchaseInfo MarkCanceled(DateTime cancelDate) {
            cancelUtcTicks = cancelDate.Kind == DateTimeKind.Utc
                ? cancelDate.Ticks
                : cancelDate.ToUniversalTime().Ticks;
            return this;
        }

    }
}