namespace Blossom.Monetization.IAP {
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    
    /// <summary>
    /// 구매 결과 이벤트 정보.
    /// </summary>
    public sealed class IAPPurchaseEventArgs {
        
        /// <summary>
        /// 구매 처리 결과 상태.
        /// </summary>
        public IAPPurchaseStatus Status { get; }

        /// <summary>
        /// 대응되는 상품 정의.
        /// </summary>
        public IAPDefinition Definition { get; }

        /// <summary>
        /// Unity IAP Product 객체.
        /// </summary>
        public Product Product { get; }

        /// <summary>
        /// 구매 실패 사유.
        /// <para/>
        /// 구매 성공 시 null이다.
        /// </summary>
        public PurchaseFailureReason? FailureReason { get; }

        /// <summary>
        /// 구매 실패 상세 메시지.
        /// </summary>
        public string FailureMessage { get; } = string.Empty;

        /// <summary>
        /// 성공 여부.
        /// </summary>
        public bool IsSuccess => Status == IAPPurchaseStatus.Succeeded;

        /// <summary>
        /// 실패 여부.
        /// </summary>
        public bool IsFailed => Status == IAPPurchaseStatus.Failed;

        internal IAPPurchaseEventArgs(IAPDefinition definition, Product product) {
            Status = IAPPurchaseStatus.Succeeded;
            Definition = definition;
            Product = product;
        }

        internal IAPPurchaseEventArgs(IAPDefinition definition, Product product, PurchaseFailureReason reason) {
            Status = IAPPurchaseStatus.Failed;
            Definition = definition;
            Product = product;
            FailureReason = reason;
        }

        internal IAPPurchaseEventArgs(IAPDefinition definition, Product product, PurchaseFailureDescription description) {
            Status = IAPPurchaseStatus.Failed;
            Definition = definition;
            Product = product;
            FailureReason = description.reason;
            FailureMessage = description.message;
        }

    }
    public enum IAPPurchaseStatus {
        Succeeded,
        Failed
    }
}