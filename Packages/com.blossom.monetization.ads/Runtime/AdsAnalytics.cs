using Blossom.Analytics;

namespace Blossom.Monetization.Ads {
    public static class AdsAnalytics {

        private static bool _registered;

        /// <summary>
        /// Ads 수익 자동 전송 브리지를 등록한다.
        /// </summary>
        public static void Register() {
            if (_registered) return;

            Ads.OnRevenuePaid += OnRevenuePaid;
            _registered = true;
        }

        /// <summary>
        /// Ads 수익 자동 전송 브리지를 해제한다.
        /// </summary>
        public static void Unregister() {
            if (!_registered) return;

            Ads.OnRevenuePaid -= OnRevenuePaid;
            _registered = false;
        }

        private static void OnRevenuePaid(AdRevenueInfo info) {
            AnalyticsAdRevenueData data = new AnalyticsAdRevenueData.Builder(
                    info.ProviderType.ToString(),
                    info.Revenue)
                .SetCurrencyCode(info.CurrencyCode)
                .SetNetworkName(info.NetworkName)
                .SetAdUnitId(info.AdUnitID)
                .SetAdType(info.AdType.ToString())
                .Build();

            Analytics.Analytics.LogAdRevenue(data);
        }

    }
}