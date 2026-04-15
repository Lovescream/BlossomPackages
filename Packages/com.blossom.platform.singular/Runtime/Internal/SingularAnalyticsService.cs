#if SDK_SINGULAR

using Blossom.Analytics;
using Singular;

namespace Blossom.Platform.Singular.Internal {
    internal sealed class SingularAnalyticsService : IAnalyticsService {

        #region Properties

        public string ServiceKey => "Singular";

        public bool IsInitialized => PlatformSingular.IsInitialized;

        #endregion

        #region Event
        
        public void LogEvent(AnalyticsEventData data) {
            if (data == null || string.IsNullOrWhiteSpace(data.EventName)) return;
            string eventName = (data.EventName ?? string.Empty).ToLowerInvariant();
            string eventString = string.IsNullOrWhiteSpace(data.ParamValue)
                ? eventName
                : $"{eventName}_{data.ParamValue}";
            SingularSDK.Event(eventString);
        }

        public void LogIAPRevenue(AnalyticsIAPRevenueData data) {
            SingularSDK.InAppPurchase(data.Product, data.Attributes);
        }

        public void LogAdRevenue(AnalyticsAdRevenueData data) {
            if (data == null) return;

            SingularAdData singularData = new SingularAdData(
                data.Platform ?? string.Empty,
                data.CurrencyCode ?? string.Empty,
                data.Revenue);

            if (!string.IsNullOrWhiteSpace(data.AdUnitId))
                singularData.WithAdUnitId(data.AdUnitId);

            if (!string.IsNullOrWhiteSpace(data.NetworkName))
                singularData.WithNetworkName(data.NetworkName);

            if (!string.IsNullOrWhiteSpace(data.PlacementName))
                singularData.WithAdPlacmentName(data.PlacementName);
            
            SingularSDK.AdRevenue(singularData);
        }

        #endregion

    }
}
#endif