namespace Blossom.Monetization.IAP.Internal {
    using System;

    internal readonly struct IAPReceiptValidationResult {

        internal bool IsValid { get; }
        internal bool IsCanceled { get; }
        internal DateTime? CancelUtc { get; }
        internal string TransactionId { get; }

        internal IAPReceiptValidationResult(bool isValid, bool isCanceled, DateTime? cancelUtc, string transactionId) {
            IsValid = isValid;
            IsCanceled = isCanceled;
            CancelUtc = cancelUtc;
            TransactionId = transactionId ?? string.Empty;
        }

    }
}