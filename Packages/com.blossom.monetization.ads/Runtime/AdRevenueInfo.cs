using System;

namespace Blossom.Monetization.Ads {
    [Serializable]
    public readonly struct AdRevenueInfo {
        /// <summary>
        /// 광고 제공자 타입
        /// </summary>
        public AdProviderType ProviderType { get; }
        
        /// <summary>
        /// 광고 타입
        /// </summary>
        public AdType AdType { get; }
        
        /// <summary>
        /// 광고 유닛 ID
        /// </summary>
        public string AdUnitID { get; }
        
        /// <summary>
        /// 광고 네트워크 이름
        /// </summary>
        public string NetworkName { get; }

        /// <summary>
        /// 수익
        /// </summary>
        public double Revenue { get; }
        
        /// <summary>
        /// 통화 코드
        /// </summary>
        public string CurrencyCode { get; }

        public AdRevenueInfo(
            AdProviderType providerType,
            AdType adType,
            string adUnitID,
            string networkName,
            double revenue,
            string currencyCode) {
            ProviderType = providerType;
            AdType = adType;
            AdUnitID = adUnitID ?? string.Empty;
            NetworkName = networkName ?? string.Empty;
            Revenue = revenue;
            CurrencyCode = currencyCode ?? string.Empty;
        }
    }
}