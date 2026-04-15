using System;

namespace Blossom.Analytics {

    [Serializable]
    public sealed class AnalyticsAdRevenueData {

        #region Properties

        public string Platform { get; }
        public string NetworkName { get; }
        public string AdUnitId { get; }
        public string PlacementName { get; }
        public string AdFormat { get; }
        public string CurrencyCode { get; }
        public double Revenue { get; }
        public string Precision { get; }
        public string AdType { get; }

        #endregion

        #region Constructor

        public AnalyticsAdRevenueData(
            string platform,
            double revenue,
            string currencyCode = null,
            string precision = null,
            string adType = null,
            string networkName = null,
            string adUnitId = null,
            string placementName = null,
            string adFormat = null) {
            Platform = platform ?? string.Empty;
            Revenue = revenue;
            CurrencyCode = currencyCode ?? string.Empty;
            Precision = precision ?? string.Empty;
            AdType = adType ?? string.Empty;
            NetworkName = networkName ?? string.Empty;
            AdUnitId = adUnitId ?? string.Empty;
            PlacementName = placementName ?? string.Empty;
            AdFormat = adFormat ?? string.Empty;
        }

        #endregion

        #region Builder

        public sealed class Builder {

            #region Properties

            public string Platform { get; private set; }
            public double Revenue { get; private set; }
            public string CurrencyCode { get; private set; }
            public string Precision { get; private set; }
            public string AdType { get; private set; }
            public string NetworkName { get; private set; }
            public string AdUnitId { get; private set; }
            public string PlacementName { get; private set; }
            public string AdFormat { get; private set; }

            #endregion

            #region Constructor

            public Builder(string platform, double revenue) {
                Platform = platform ?? string.Empty;
                Revenue = revenue;
            }

            #endregion

            #region Set

            public Builder SetCurrencyCode(string currencyCode) {
                CurrencyCode = currencyCode ?? string.Empty;
                return this;
            }

            public Builder SetPrecision(string precision) {
                Precision = precision ?? string.Empty;
                return this;
            }

            public Builder SetAdType(string adType) {
                AdType = adType ?? string.Empty;
                return this;
            }

            public Builder SetNetworkName(string networkName) {
                NetworkName = networkName ?? string.Empty;
                return this;
            }

            public Builder SetAdUnitId(string adUnitId) {
                AdUnitId = adUnitId ?? string.Empty;
                return this;
            }

            public Builder SetPlacementName(string placementName) {
                PlacementName = placementName ?? string.Empty;
                return this;
            }

            public Builder SetAdFormat(string adFormat) {
                AdFormat = adFormat ?? string.Empty;
                return this;
            }

            #endregion

            #region Build

            public AnalyticsAdRevenueData Build() {
                return new AnalyticsAdRevenueData(Platform, Revenue, CurrencyCode, Precision, AdType, NetworkName,
                    AdUnitId, PlacementName, AdFormat);
            }

            #endregion

        }

        #endregion

    }
}