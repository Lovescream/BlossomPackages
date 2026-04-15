using System;
using System.Collections.Generic;

namespace Blossom.Analytics.Internal {
    internal static class AnalyticsTargetResolver {
        internal static IReadOnlyList<IAnalyticsService> Resolve(List<IAnalyticsService> allServices,
            Dictionary<string, IAnalyticsService> serviceMap, params string[] targetServiceKeys) {
            if (allServices == null || allServices.Count <= 0) return Array.Empty<IAnalyticsService>();
            if (targetServiceKeys == null || targetServiceKeys.Length <= 0) return allServices;
            List<IAnalyticsService> result = new();
            foreach (string key in targetServiceKeys) {
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!serviceMap.TryGetValue(key, out IAnalyticsService service)) continue;
                if (service == null) continue;
                if (result.Contains(service)) continue;

                result.Add(service);
            }

            return result;
        }
    }
}