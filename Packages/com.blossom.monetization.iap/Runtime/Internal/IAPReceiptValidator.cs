namespace Blossom.Monetization.IAP.Internal {
    using System;
    using UnityEngine;
    using UnityEngine.Purchasing.Security;

    internal sealed class IAPReceiptValidator {

        #region Fields

        private readonly byte[] _googleTangle;
        private readonly byte[] _appleTangle;

        #endregion

        #region Constructor

        internal IAPReceiptValidator(byte[] googleTangle, byte[] appleTangle) {
            _googleTangle = googleTangle;
            _appleTangle = appleTangle;
        }

        #endregion

        #region Validate

        internal bool Validate(string receipt, string productId) {
            return ValidateDetailed(receipt, productId).IsValid;
        }

        internal IAPReceiptValidationResult ValidateDetailed(string receipt, string productId) {
            try {
                CrossPlatformValidator validator = new(_googleTangle, _appleTangle, Application.identifier);
                IPurchaseReceipt[] receipts = validator.Validate(receipt);

                // result 안에 같은 productID가 여러 번 있을 수 있음. 가장 최신 영수증을 기준으로 판단.
#if UNITY_ANDROID
                GooglePlayReceipt latestGoogleReceipt = null;
                foreach (IPurchaseReceipt purchaseReceipt in receipts) {
                    if (purchaseReceipt is not GooglePlayReceipt googleReceipt) continue;
                    if (!string.Equals(googleReceipt.productID, productId, StringComparison.Ordinal)) continue;

                    if (latestGoogleReceipt == null || latestGoogleReceipt.purchaseDate < googleReceipt.purchaseDate)
                        latestGoogleReceipt = googleReceipt;
                }

                if (latestGoogleReceipt == null)
                    return new IAPReceiptValidationResult(false, false, null, string.Empty);

                bool isCanceled = latestGoogleReceipt.purchaseState != GooglePurchaseState.Purchased;
                bool isValid = latestGoogleReceipt.purchaseState == GooglePurchaseState.Purchased;
                DateTime? cancelUtc = isCanceled ? DateTime.UtcNow : null;
                return new IAPReceiptValidationResult(isValid, isCanceled, cancelUtc,
                    latestGoogleReceipt.transactionID ?? string.Empty);
#elif UNITY_IOS
                AppleInAppPurchaseReceipt latestAppleReceipt = null;
                foreach (IPurchaseReceipt purchaseReceipt in receipts) {
                    if (purchaseReceipt is not AppleInAppPurchaseReceipt appleReceipt) continue;
                    if (!string.Equals(appleReceipt.productID, productId, StringComparison.Ordinal)) continue;

                    if (latestAppleReceipt == null || latestAppleReceipt.purchaseDate < appleReceipt.purchaseDate)
                        latestAppleReceipt = appleReceipt;
                }

                if (latestAppleReceipt == null)
                    return new IAPReceiptValidationResult(false, false, null, string.Empty);

                bool isCanceled = latestAppleReceipt.cancellationDate != DateTime.MinValue;
                DateTime? cancelUtc = isCanceled
                    ? DateTime.SpecifyKind(latestAppleReceipt.cancellationDate, DateTimeKind.Utc)
                    : null;
                return new IAPReceiptValidationResult(!isCanceled, isCanceled, cancelUtc,
                    latestAppleReceipt.transactionID ?? string.Empty);
#else
                return new IAPReceiptValidationResult(false, false, null, string.Empty);
#endif
            }
            catch (Exception e) {
                Debug.LogError($"[Blossom:IAP] Receipt validation failed: {e}");
                return new IAPReceiptValidationResult(false, false, null, string.Empty);
            }
        }

        #endregion

    }
}