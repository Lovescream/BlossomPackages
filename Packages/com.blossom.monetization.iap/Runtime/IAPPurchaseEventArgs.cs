using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Blossom.Monetization.IAP {
    public sealed class IAPPurchaseEventArgs {
        
        public IAPPurchaseStatus Status { get; }
        public IAPDefinition Definition { get; }
        public Product Product { get; }
        public PurchaseFailureReason? FailureReason { get; }
        public string FailureMessage { get; } = string.Empty;
        
        public bool IsSuccess => Status == IAPPurchaseStatus.Succeeded;
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
}