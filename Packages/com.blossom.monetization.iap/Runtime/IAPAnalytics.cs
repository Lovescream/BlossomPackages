namespace Blossom.Monetization.IAP {
    using System.Collections.Generic;
    using Analytics;
    using UnityEngine;
    
    public static class IAPAnalytics {

        private static bool _registered;

        /// <summary>
        /// IAP Revenue 자동 전송 브리지를 등록한다.
        /// </summary>
        public static void Register() {
            if (_registered) return;

            IAP.OnPurchaseResult += OnPurchaseResult;
            _registered = true;
        }

        /// <summary>
        /// IAP Revenue 자동 전송 브리지를 해제한다.
        /// </summary>
        public static void Unregister() {
            if (!_registered) return;

            IAP.OnPurchaseResult -= OnPurchaseResult;
            _registered = false;
        }

        private static void OnPurchaseResult(IAPPurchaseEventArgs args) {
            if (args == null) return;
            if (!args.IsSuccess) return;
            if (args.Product == null) return;

            string storeKey = GetStoreKey();
            string revenue = args.Product.metadata != null
                ? args.Product.metadata.localizedPriceString
                : string.Empty;

            Dictionary<string, object> attributes = new();
            if (!string.IsNullOrWhiteSpace(revenue)) {
                attributes["Revenue"] = revenue;
            }
            
            AnalyticsIAPRevenueData data = new AnalyticsIAPRevenueData.Builder(storeKey, args.Product)
                .SetRevenue(revenue).SetAttributes(attributes).Build();

            Analytics.LogIAPRevenue(data);
        }

        private static string GetStoreKey() {
            return Application.platform == RuntimePlatform.Android
                ? "GooglePlay"
                : "AppleAppStore";
        }

    }
}