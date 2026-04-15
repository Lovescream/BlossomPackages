#if SDK_FIREBASE && SDK_FIREBASE_ANALYTICS

using System.Collections.Generic;
using Blossom.Analytics;
using Firebase.Analytics;

namespace Blossom.Platform.Firebase.Internal {
    internal sealed class FirebaseAnalyticsService : IAnalyticsService {

        #region Properties

        public string ServiceKey => "Firebase";

        public bool IsInitialized => PlatformFirebaseAnalytics.IsInitialized;

        #endregion

        #region Event

        public void LogEvent(AnalyticsEventData data) {
            if (data == null) return;
            if (string.IsNullOrWhiteSpace(data.EventName)) return;
            string eventName = data.EventName.ToLowerInvariant();

            List<Parameter> parameters = new();
            if (!string.IsNullOrWhiteSpace(data.ParamValue)) parameters.Add(new Parameter(eventName, data.ParamValue));
            AddAttributes(parameters, data.Attributes);

            if (parameters.Count > 0) PlatformFirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
            else PlatformFirebaseAnalytics.LogEvent(eventName);
        }

        public void LogIAPRevenue(AnalyticsIAPRevenueData data) {
            if (data == null) return;

            Parameter[] parameters = {
                new(FirebaseAnalytics.ParameterAdPlatform, data.StoreKey),
                new(FirebaseAnalytics.ParameterItemName, data.ProductId),
                new(FirebaseAnalytics.EventPurchase, data.Revenue)
            };

            PlatformFirebaseAnalytics.LogEvent("in_app_purchase", parameters);
        }

        public void LogAdRevenue(AnalyticsAdRevenueData data) {
            if (data == null) return;

            Parameter[] parameters;
            if (!string.IsNullOrWhiteSpace(data.Precision)) {
                parameters = new[] {
                    new Parameter("ad_impression", data.AdType),
                    new Parameter("ad_platform", data.Platform),
                    new Parameter("value", data.Revenue),
                    new Parameter("estimated", data.Precision),
                    new Parameter("currency", data.CurrencyCode)
                };
            }
            else {
                parameters = new[] {
                    new Parameter("ad_type", data.AdType),
                    new Parameter("ad_platform", data.Platform),
                    new Parameter("ad_source", data.NetworkName),
                    new Parameter("ad_unit_name", data.AdUnitId),
                    new Parameter("ad_format", data.AdFormat),
                    new Parameter("value", data.Revenue),
                    new Parameter("currency", data.CurrencyCode)
                };
            }

            PlatformFirebaseAnalytics.LogEvent("ad_impression", parameters);
        }

        #endregion

        private static void AddAttributes(List<Parameter> parameters, IReadOnlyDictionary<string, object> attributes) {
            if (parameters == null) return;
            if (attributes == null || attributes.Count == 0) return;

            foreach (KeyValuePair<string, object> pair in attributes) {
                if (string.IsNullOrWhiteSpace(pair.Key)) continue;
                if (pair.Value == null) continue;

                string key = pair.Key;
                object value = pair.Value;

                switch (value) {
                    case string stringValue:
                        parameters.Add(new Parameter(key, stringValue));
                        break;

                    case int intValue:
                        parameters.Add(new Parameter(key, intValue));
                        break;

                    case long longValue:
                        parameters.Add(new Parameter(key, longValue));
                        break;

                    case float floatValue:
                        parameters.Add(new Parameter(key, (double)floatValue));
                        break;

                    case double doubleValue:
                        parameters.Add(new Parameter(key, doubleValue));
                        break;

                    case bool boolValue:
                        parameters.Add(new Parameter(key, boolValue ? 1L : 0L));
                        break;

                    default:
                        parameters.Add(new Parameter(key, value.ToString()));
                        break;
                }
            }
        }

    }
}
#endif