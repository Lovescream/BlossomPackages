using System;
using Blossom.Common;
using UnityEngine;

namespace Blossom.Monetization.Ads {

    public sealed class AdsSettingsSO : SettingsSOBase {
        public AdProviderSettings Providers => providers;
        public AdsRuntimeSettings Runtime => runtime;
        public AppLovinAdsSettings AppLovin => appLovin;
        public AdMobAdsSettings AdMob => adMob;
        
        [SerializeField] private AdProviderSettings providers;
        [SerializeField] private AdsRuntimeSettings runtime;
        [SerializeField] private AppLovinAdsSettings appLovin;
        [SerializeField] private AdMobAdsSettings adMob;
    }
    
    [Serializable]
    public sealed class AdProviderSettings {
        public AdProviderType AppOpen => appOpen;
        public AdProviderType Banner => banner;
        public AdProviderType Interstitial => interstitial;
        public AdProviderType Rewarded => rewarded;
        
        [SerializeField] private AdProviderType appOpen = AdProviderType.None;
        [SerializeField] private AdProviderType banner = AdProviderType.None;
        [SerializeField] private AdProviderType interstitial = AdProviderType.None;
        [SerializeField] private AdProviderType rewarded = AdProviderType.None;
    }

    [Serializable]
    public sealed class AdsRuntimeSettings {
        public int LoadMaxAttempts => loadMaxAttempts;
        public float InitializeTimeout => initializeTimeout;
        public bool UseNetworkValidation => useNetworkValidation;
        
        [SerializeField, Tooltip("광고 로드 실패 시 최대 재시도 횟수")] private int loadMaxAttempts = 3;
        [SerializeField, Tooltip("광고 시스템 초기화 타임아웃 시간")] private float initializeTimeout = 10f;
        [SerializeField, Tooltip("네트워크 연결 검증 사용 여부")] private bool useNetworkValidation = false;
    }

    [Serializable]
    public sealed class AppLovinAdsSettings {
#if UNITY_ANDROID
        public string BannerID => aosBannerID;
        public string InterstitialID => aosInterstitialID;
        public string RewardedID => aosRewardedID;
#elif UNITY_IOS
        public string BannerID => iosBannerID;
        public string InterstitialID => iosInterstitialID;
        public string RewardedID => iosRewardedID;
#else
        public string BannerID => "";
        public string InterstitialID => "";
        public string RewardedID => "";
#endif
        [SerializeField] private string aosBannerID;
        [SerializeField] private string iosBannerID;
        [SerializeField] private string aosInterstitialID;
        [SerializeField] private string iosInterstitialID;
        [SerializeField] private string aosRewardedID;
        [SerializeField] private string iosRewardedID;
    }

    [Serializable]
    public sealed class AdMobAdsSettings {
#if UNITY_ANDROID
        public string AppOpenID => aosAppOpenID;
        public string BannerID => aosBannerID;
        public string InterstitialID => aosInterstitialID;
        public string RewardedID => aosRewardedID;
#elif UNITY_IOS
        public string AppOpenID => iosAppOpenID;
        public string BannerID => iosBannerID;
        public string InterstitialID => iosInterstitialID;
        public string RewardedID => iosRewardedID;
#else
        public string AppOpenID => "";
        public string BannerID => "";
        public string InterstitialID => "";
        public string RewardedID => "";
#endif
        [SerializeField] private string aosAppOpenID;
        [SerializeField] private string iosAppOpenID;
        [SerializeField] private string aosBannerID;
        [SerializeField] private string iosBannerID;
        [SerializeField] private string aosInterstitialID;
        [SerializeField] private string iosInterstitialID;
        [SerializeField] private string aosRewardedID;
        [SerializeField] private string iosRewardedID;
    }

}