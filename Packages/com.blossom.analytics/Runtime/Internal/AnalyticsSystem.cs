namespace Blossom.Analytics.Internal {
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    
    internal static class AnalyticsSystem {

        #region Properties

        internal static bool IsInitialized { get; private set; }
        internal static AnalyticsSettings Settings { get; private set; }
        internal static IReadOnlyList<string> RegisteredServiceKeys => _registeredServiceKeys;

        #endregion
        
        #region Fields

        private static readonly List<IAnalyticsService> _services = new List<IAnalyticsService>();

        private static readonly Dictionary<string, IAnalyticsService> _serviceDictionary =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly List<string> _registeredServiceKeys = new();

        #endregion

        #region Initialize

        internal static void Initialize(AnalyticsSettings settings = null) {
            if (IsInitialized) return;

            Settings = settings ?? new();
            IsInitialized = true;
        }

        #endregion

        #region Register

        internal static void RegisterService(IAnalyticsService service) {
            if (service == null) {
                Debug.LogError("[Blossom:Analytics] RegisterService(): service is null.");
                return;
            }
            
            string serviceKey = service.ServiceKey;
            if (string.IsNullOrWhiteSpace(serviceKey)) {
                Debug.LogError("[Blossom:Analytics] RegisterService(): service key is empty.");
                return;
            }

            if (_serviceDictionary.ContainsKey(serviceKey)) return;
            
            _services.Add(service);
            _serviceDictionary.Add(serviceKey, service);
            _registeredServiceKeys.Add(serviceKey);
        }

        internal static void ClearServices() {
            _services.Clear();
            _serviceDictionary.Clear();
            _registeredServiceKeys.Clear();
        }

        #endregion

        #region Event

        internal static void LogEvent(AnalyticsEventData data, params string[] targetServiceKeys) {
            if (data == null) {
                Debug.LogWarning("[Blossom:Analytics] LogEvent(): data is null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(data.EventName)) {
                Debug.LogWarning("[Blossom:Analytics] LogEvent(): EventName is empty.");
                return;
            }

            IReadOnlyList<IAnalyticsService> targets =
                AnalyticsTargetResolver.Resolve(_services, _serviceDictionary, targetServiceKeys);
            DispatchEvent(targets, data);
        }

        internal static void LogIAPRevenue(AnalyticsIAPRevenueData data, params string[] targetServiceKeys) {
            if (data == null) {
                Debug.LogWarning("[Blossom:Analytics] LogIAPRevenue(): data is null.");
                return;
            }

            IReadOnlyList<IAnalyticsService> targets =
                AnalyticsTargetResolver.Resolve(_services, _serviceDictionary, targetServiceKeys);
            DispatchIAPRevenue(targets, data);
        }

        internal static void LogAdRevenue(AnalyticsAdRevenueData data, params string[] targetServiceKeys) {
            if (data == null) {
                Debug.LogWarning("[Blossom:Analytics] LogAdRevenue(): data is null.");
                return;
            }

            IReadOnlyList<IAnalyticsService> targets =
                AnalyticsTargetResolver.Resolve(_services, _serviceDictionary, targetServiceKeys);
            DispatchAdRevenue(targets, data);
        }

        #endregion

        #region Dispatch

        private static void DispatchEvent(IReadOnlyList<IAnalyticsService> targets, AnalyticsEventData data) {
            foreach (IAnalyticsService service in targets) {
                if (service == null) continue;
                if (!service.IsInitialized) {
                    Debug.LogWarning("[Blossom:Analytics] DispatchEvent(): service is not initialized.");
                    continue;
                }

                try {
                    service.LogEvent(data);
                }
                catch (Exception e) {
                    Debug.LogError(
                        $"[Blossom:Analytics] LogEvent Failed. (Key: {service.ServiceKey}, Event: {data.EventName}) : {e}");
                }
            }
        }

        private static void DispatchIAPRevenue(IReadOnlyList<IAnalyticsService> targets, AnalyticsIAPRevenueData data) {
            foreach (IAnalyticsService service in targets) {
                if (service == null) continue;
                if (!service.IsInitialized) {
                    Debug.LogWarning("[Blossom:Analytics] DispatchEvent(): service is not initialized.");
                    continue;
                }

                try {
                    service.LogIAPRevenue(data);
                }
                catch (Exception e) {
                    Debug.LogError(
                        $"[Blossom:Analytics] LogIAPRevenue Failed. (Key: {service.ServiceKey}, ProductId: {data.ProductId}) : {e}");
                }
            }
        }

        private static void DispatchAdRevenue(IReadOnlyList<IAnalyticsService> targets, AnalyticsAdRevenueData data) {
            foreach (IAnalyticsService service in targets) {
                if (service == null) continue;
                if (!service.IsInitialized) {
                    Debug.LogWarning("[Blossom:Analytics] DispatchEvent(): service is not initialized.");
                    continue;
                }

                try {
                    service.LogAdRevenue(data);
                }
                catch (Exception e) {
                    Debug.LogError(
                        $"[Blossom:Analytics] LogAdRevenue Failed. (Key: {service.ServiceKey}, Platform: {data.Platform}) : {e}");
                }
            }
        }

        #endregion

    }
}