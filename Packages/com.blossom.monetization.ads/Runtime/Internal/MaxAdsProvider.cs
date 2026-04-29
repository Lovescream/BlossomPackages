#if SDK_APPLOVINMAX

namespace Blossom.Monetization.Ads.Internal {
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    
    internal sealed class MaxAdsProvider : IAdsProvider {

        #region Properties

        public AdProviderType ProviderType => AdProviderType.AppLovin;
        public bool IsInitialized { get; private set; }

        #endregion

        #region Fields

        private MaxInterstitialAdUnit _interstitial;
        private MaxRewardedAdUnit _rewarded;
        private MaxBannerAdUnit _banner;

        #endregion

        #region Initialize

        public async UniTask<bool> InitializeAsync(AdsSettingsSO settings) {
            if (IsInitialized) return true;
            if (settings == null) return false;
            
            float timeout = Time.time + settings.Runtime.InitializeTimeout;
            while (!Platform.AppLovinMax.PlatformAppLovinMax.IsInitialized) {
                if (Time.time > timeout) {
                    Debug.LogError($"[Blossom:Ads:MaxAdsProvider] InitializeAsync(): AppLovin Initialize Timeout.");
                    return false;
                }
                await UniTask.NextFrame();
            }

            MaxSdk.SetMuted(true);

            _interstitial = new(settings);
            _rewarded = new(settings);
            _banner = new(settings);

            IsInitialized = true;
            return true;
        }

        #endregion

        #region Ad Units


        public bool Supports(AdType adType) {
            return adType is AdType.Banner or AdType.Interstitial or AdType.Rewarded;
        }

        public AdUnitBase GetAdUnit(AdType adType) {
            return adType switch {
                AdType.Banner => _banner,
                AdType.Interstitial => _interstitial,
                AdType.Rewarded => _rewarded,
                _ => null
            };
        }
        
        #endregion

    }
}

#endif