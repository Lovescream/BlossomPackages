namespace Blossom.Analytics {
    public sealed class AnalyticsSettings {

        public bool AutoSendAdRevenue { get; }
        public bool AutoSendIAPRevenue { get; }

        public AnalyticsSettings(bool autoSendAdRevenue = true, bool autoSendIAPRevenue = true) {
            AutoSendAdRevenue = autoSendAdRevenue;
            AutoSendIAPRevenue = autoSendIAPRevenue;
        }

    }
}