#if SDK_GAMEANALYTICS

namespace Blossom.Platform.GameAnalytics.Internal {
    using System.Collections.Generic;
    using Analytics;
    using GameAnalyticsSDK;
    
    internal sealed class GameAnalyticsService : IAnalyticsService {

        #region Properties

        public string ServiceKey => "GameAnalytics";

        public bool IsInitialized => PlatformGameAnalytics.IsInitialized;

        #endregion

        #region Event

        public void LogEvent(AnalyticsEventData data) {
            if (data == null || string.IsNullOrWhiteSpace(data.EventName)) return;
            
            string eventName = data.EventName.ToLowerInvariant();
            string designEvent = string.IsNullOrWhiteSpace(data.ParamValue)
                ? eventName
                : $"{eventName}:{data.ParamValue}";

            GameAnalyticsSDK.GameAnalytics.NewDesignEvent(designEvent);
        }

        public void LogIAPRevenue(AnalyticsIAPRevenueData data) {
            if (data == null) return;
            GameAnalyticsSDK.GameAnalytics.NewBusinessEvent("USD", 1,
                data.Product.definition.type.ToString(),
                data.ProductId,
                data.StoreKey,
                data.Attributes);
        }

        public void LogAdRevenue(AnalyticsAdRevenueData data) {
            if (data == null) return;

            Dictionary<string, object> parameters = new() {
                { "currency", data.CurrencyCode ?? string.Empty },
                { "revenue", data.Revenue }
            };

            if (!string.IsNullOrWhiteSpace(data.Precision)) {
                parameters["estimated"] = data.Precision;
            }

            GAAdType adType = ConvertToAdType(data.AdType);

            GameAnalyticsSDK.GameAnalytics.NewAdEvent(
                GAAdAction.Show,
                adType,
                data.Platform ?? string.Empty,
                data.NetworkName ?? string.Empty,
                parameters);
        }

        #endregion
        
        private static GAAdType ConvertToAdType(string adType) {
            if (string.IsNullOrWhiteSpace(adType)) return GAAdType.Undefined;

            return adType.ToLowerInvariant() switch {
                "banner" => GAAdType.Banner,
                "interstitial" => GAAdType.Interstitial,
                "rewarded" => GAAdType.RewardedVideo,
                "appopen" => GAAdType.AppOpen,
                _ => GAAdType.Undefined
            };
        }

    }
}

#endif