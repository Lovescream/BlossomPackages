namespace Blossom.Monetization.Ads {
    using System;
    using Internal;
    
    public static class Ads {

        #region Properties

        /// <summary>
        /// 광고 시스템이 초기화되었는지 여부.
        /// </summary>
        public static bool IsInitialized => AdsSystem.IsInitialized;
        
        #endregion

        #region Events

        /// <summary>
        /// 광고 제공자 초기화 완료 시의 이벤트.
        /// </summary>
        public static event Action<AdProviderType> OnProviderInitialized {
            add => AdsSystem.OnProviderInitialized += value;
            remove => AdsSystem.OnProviderInitialized -= value;
        }
        
        /// <summary>
        /// 광고 로드 요청이 발생했을 때의 이벤트.
        /// </summary>
        public static event Action<AdProviderType, AdType> OnAdLoadRequested {
            add => AdsSystem.OnAdLoadRequested += value;
            remove => AdsSystem.OnAdLoadRequested -= value;
        }
        
        /// <summary>
        /// 광고 로드 완료 시의 이벤트.
        /// </summary>
        public static event Action<AdProviderType, AdType> OnAdLoaded {
            add => AdsSystem.OnAdLoaded += value;
            remove => AdsSystem.OnAdLoaded -= value;
        }

        /// <summary>
        /// 광고가 실제로 화면에 표시되었을 때의 이벤트.
        /// </summary>
        public static event Action<AdProviderType, AdType> OnAdDisplayed {
            add => AdsSystem.OnAdDisplayed += value;
            remove => AdsSystem.OnAdDisplayed -= value;
        }

        /// <summary>
        /// 광고가 종료되었거나 닫혔을 때의 이벤트.
        /// </summary>
        public static event Action<AdProviderType, AdType> OnAdClosed {
            add => AdsSystem.OnAdClosed += value;
            remove => AdsSystem.OnAdClosed -= value;
        }
        
        /// <summary>
        /// 광고 수익 발생 시의 이벤트.
        /// </summary>
        public static event Action<AdRevenueInfo> OnRevenuePaid {
            add => AdsSystem.OnAdRevenuePaid += value;
            remove => AdsSystem.OnAdRevenuePaid -= value;
        }
        
        #endregion

        #region Initialize

        /// <summary>
        /// 광고 시스템을 초기화함.
        /// </summary>
        /// <param name="settings">광고 설정</param>
        public static void Initialize(AdsSettingsSO settings = null) => AdsSystem.Initialize(settings);

        #endregion

        #region Load

        /// <summary>
        /// 지정한 타입의 광고 로드를 요청.
        /// </summary>
        /// <param name="adType">로드할 광고 타입</param>
        public static void RequestAd(AdType adType) => AdsSystem.RequestAd(adType);

        /// <summary>
        /// 지정한 타입의 광고가 표시 가능한 상태로 로드되었는지 확인.
        /// </summary>
        /// <param name="adType">확인할 광고 타입</param>
        /// <returns>로드되어 있다면 true, 아니면 false</returns>
        public static bool IsLoaded(AdType adType) => AdsSystem.IsAdLoaded(adType);

        #endregion

        #region Show / Hide / Destroy

        /// <summary>
        /// 지정한 타입의 광고를 표시함. (배너는 <see cref="ShowBanner"/>를 사용하여 표시할 것)
        /// </summary>
        /// <param name="adType">표시할 광고 타입</param>
        /// <param name="callback">광고 표시 흐름 완료 시 콜백</param>
        public static void ShowAd(AdType adType, Action<bool> callback = null) => AdsSystem.ShowAd(adType, callback);
        
        /// <summary>
        /// 배너 광고 표시.
        /// </summary>
        /// <param name="callback">광고 표시 흐름 완료 시 콜백</param>
        public static void ShowBanner(Action<bool> callback = null) => AdsSystem.ShowBanner(callback);
        
        /// <summary>
        /// 지정한 타입의 광고를 숨김.
        /// </summary>
        /// <param name="adType">숨길 광고 타입</param>
        public static void HideAd(AdType adType) => AdsSystem.HideAd(adType);
        
        /// <summary>
        /// 지정한 타입의 광고를 제거.
        /// </summary>
        /// <param name="adType">제거할 광고 타입</param>
        public static void DestroyAd(AdType adType) => AdsSystem.DestroyAd(adType);

        #endregion

    }
}