namespace Blossom.Analytics {
    public interface IAnalyticsService {
        string ServiceKey { get; }
        bool IsInitialized { get; }
        void LogEvent(AnalyticsEventData data);
        void LogIAPRevenue(AnalyticsIAPRevenueData data);
        void LogAdRevenue(AnalyticsAdRevenueData data);
    }
}