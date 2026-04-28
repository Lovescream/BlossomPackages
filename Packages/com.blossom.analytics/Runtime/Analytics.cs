using System.Collections.Generic;
using Blossom.Analytics.Internal;

namespace Blossom.Analytics {
    public static class Analytics {

        #region Properties

        /// <summary>
        /// Analytics 시스템 초기화 여부.
        /// </summary>
        public static bool IsInitialized => AnalyticsSystem.IsInitialized;

        /// <summary>
        /// 현재 등록된 서비스 키 목록.
        /// </summary>
        public static IReadOnlyList<string> RegisteredServiceKeys => AnalyticsSystem.RegisteredServiceKeys;

        /// <summary>
        /// 현재 설정.
        /// </summary>
        public static AnalyticsSettings Settings => AnalyticsSystem.Settings;

        #endregion

        #region Initialize

        /// <summary>
        /// Analytics 시스템을 초기화한다.
        /// </summary>
        /// <param name="settings">초기 설정. null이면 기본값 사용.</param>
        public static void Initialize(AnalyticsSettings settings = null) {
            AnalyticsSystem.Initialize(settings);
        }

        #endregion

        #region Register

        /// <summary>
        /// 분석 서비스를 등록한다.
        /// </summary>
        /// <param name="service">등록할 서비스</param>
        public static void RegisterService(IAnalyticsService service) {
            AnalyticsSystem.RegisterService(service);
        }

        /// <summary>
        /// 등록된 모든 분석 서비스를 제거한다.
        /// </summary>
        public static void ClearServices() {
            AnalyticsSystem.ClearServices();
        }

        #endregion

        #region Event

        /// <summary>
        /// 일반 이벤트를 전송한다.
        /// targetServiceKeys를 지정하지 않으면 등록된 모든 서비스로 전송한다.
        /// </summary>
        public static void LogEvent(
            string eventName,
            string paramName = null,
            string paramValue = null,
            IReadOnlyDictionary<string, object> attributes = null,
            params string[] targetServiceKeys) {
            AnalyticsEventData data = new AnalyticsEventData(eventName, paramName, paramValue, attributes);
            AnalyticsSystem.LogEvent(data, targetServiceKeys);
        }

        /// <summary>
        /// 일반 이벤트 데이터를 전송한다.
        /// targetServiceKeys를 지정하지 않으면 등록된 모든 서비스로 전송한다.
        /// </summary>
        public static void LogEvent(AnalyticsEventData data, params string[] targetServiceKeys) {
            AnalyticsSystem.LogEvent(data, targetServiceKeys);
        }

        #endregion

        #region Revenue

        /// <summary>
        /// IAP Revenue 데이터를 전송한다.
        /// targetServiceKeys를 지정하지 않으면 등록된 모든 서비스로 전송한다.
        /// </summary>
        public static void LogIAPRevenue(AnalyticsIAPRevenueData data, params string[] targetServiceKeys) {
            AnalyticsSystem.LogIAPRevenue(data, targetServiceKeys);
        }

        /// <summary>
        /// Ad Revenue 데이터를 전송한다.
        /// targetServiceKeys를 지정하지 않으면 등록된 모든 서비스로 전송한다.
        /// </summary>
        public static void LogAdRevenue(AnalyticsAdRevenueData data, params string[] targetServiceKeys) {
            AnalyticsSystem.LogAdRevenue(data, targetServiceKeys);
        }

        #endregion

    }
}