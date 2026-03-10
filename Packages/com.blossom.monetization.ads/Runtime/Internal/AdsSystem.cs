using System;
using System.Collections.Generic;
using Blossom.Common;
using Blossom.Networking.Online;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Blossom.Monetization.Ads.Internal {
    internal static class AdsSystem {
        
        #region Properties

        internal static bool IsInitialized { get; private set; }
        internal static AdsSettingsSO Settings { get; private set; }

        #endregion

        #region Fields

        private static readonly Dictionary<AdProviderType, IAdsProvider> Providers = new();

        private static bool _appOpenAvailable = true;
        
        #endregion

        #region Events

        internal static event Action<AdProviderType> OnProviderInitialized;
        internal static event Action<AdProviderType, AdType> OnAdLoadRequested;
        internal static event Action<AdProviderType, AdType> OnAdLoaded;
        internal static event Action<AdProviderType, AdType> OnAdDisplayed;
        internal static event Action<AdProviderType, AdType> OnAdClosed;
        internal static event Action<AdRevenueInfo> OnAdRevenuePaid;

        #endregion

        #region Initialize

        internal static void Initialize(AdsSettingsSO settings = null) {
            if (IsInitialized) return;

            Settings = settings ?? SettingsLoader.Get<AdsSettingsSO>();
            if (Settings == null) {
                Debug.LogError($"[Blossom:Ads] Initialize(): SettingsSO not found.");
                return;
            }
            
            Providers.Clear();
            SetProviders();
            _appOpenAvailable = true;
            InitializeProvidersAsync().Forget();

            IsInitialized = true;
        }
        
        // 사용 가능한 광고 제공자 등록.
        private static void SetProviders() {
            List<IAdsProvider> providers = new();
#if SDK_APPLOVINMAX
            providers.Add(new MaxAdsProvider());
#endif
#if SDK_ADMOB
            providers.Add(new AdMobAdsProvider());
#endif

            foreach (IAdsProvider provider in providers) {
                if (provider == null) continue;
                Providers.TryAdd(provider.ProviderType, provider);
            }
        }

        // 등록된 광고 제공자들을 비동기로 초기화.
        private static async UniTaskVoid InitializeProvidersAsync() {
            foreach (KeyValuePair<AdProviderType, IAdsProvider> pair in Providers) {
                IAdsProvider provider = pair.Value;
                if (provider == null) continue;
                if (!IsProviderUsed(provider.ProviderType)) continue;

                bool initialized = await provider.InitializeAsync(Settings);
                if (!initialized) continue;

                OnProviderInitialized?.Invoke(provider.ProviderType);
                BindProviderEvents(provider.ProviderType, provider);
                
                // 초기화 완료 즉시 필요한 광고를 선로드.
                RequestAdInternal(AdType.AppOpen);
                RequestAdInternal(AdType.Banner);
                RequestAdInternal(AdType.Interstitial);
                RequestAdInternal(AdType.Rewarded);
            }
        }

        private static void BindProviderEvents(AdProviderType providerType, IAdsProvider provider) {
            if (provider == null) return;
            
            BindAdUnitEvents(providerType, provider.GetAdUnit(AdType.AppOpen));
            BindAdUnitEvents(providerType, provider.GetAdUnit(AdType.Banner));
            BindAdUnitEvents(providerType, provider.GetAdUnit(AdType.Interstitial));
            BindAdUnitEvents(providerType, provider.GetAdUnit(AdType.Rewarded));
        }

        private static void BindAdUnitEvents(AdProviderType providerType, AdUnitBase unit) {
            if (unit == null) return;

            unit.OnLoaded += () => OnAdLoaded?.Invoke(providerType, unit.AdType);
            unit.OnDisplayed += () => OnAdDisplayed?.Invoke(providerType, unit.AdType);
            unit.OnClosed += () => {
                OnAdClosed?.Invoke(providerType, unit.AdType);
                if (unit.AdType is AdType.Interstitial or AdType.Rewarded) {
                    _appOpenAvailable = true;
                    RequestAd(unit.AdType);
                }
            };
            unit.OnRevenuePaid += info => OnAdRevenuePaid?.Invoke(info);
        }

        #endregion

        #region Ad

        internal static bool IsAdLoaded(AdType adType) {
            if (!CheckInitialized()) return false;
            return TryGetAdUnit(adType, out AdProviderType _, out AdUnitBase unit) && unit.IsLoaded;
        }

        internal static void ShowAd(AdType adType, Action<bool> callback = null) {
            if (!CheckInitialized()) {
                callback?.Invoke(false);
                return;
            }
            if (adType == AdType.AppOpen && !_appOpenAvailable) {
                callback?.Invoke(false);
                return;
            }

            if (!TryGetAdUnit(adType, out AdProviderType _, out AdUnitBase unit) || !unit.IsLoaded) {
                callback?.Invoke(false);
                return;
            }

            if (adType is AdType.Interstitial or AdType.Rewarded) _appOpenAvailable = false;

            unit.Show(callback);
        }
        
        internal static void ShowBanner(Action<bool> callback = null) {
            if (!CheckInitialized()) {
                callback?.Invoke(false);
                return;
            }

            if (!TryGetAdUnit(AdType.Banner, out AdProviderType _, out AdUnitBase unit)) {
                callback?.Invoke(false);
                return;
            }

            if (!unit.IsLoaded) {
                RequestAdInternal(AdType.Banner);
                callback?.Invoke(false);
                return;
            }
            
            unit.Show(callback);
        }

        internal static void HideAd(AdType adType) {
            if (!CheckInitialized()) return;
            if (!TryGetAdUnit(adType, out AdProviderType _, out AdUnitBase unit)) return;
            
            unit.Hide();
        }
        
        internal static void DestroyAd(AdType adType) {
            if (!CheckInitialized()) return;
            if (!TryGetAdUnit(adType, out AdProviderType _, out AdUnitBase unit)) return;
            
            unit.Destroy();
        }
        
        #endregion
        
        #region Common

        // 시스템 초기화 여부 검사.
        private static bool CheckInitialized() {
            if (IsInitialized) return true;
            Debug.LogWarning("[Blossom:Ads] AdsSystem has not yet initialized.");
            return false;
        }
        
        // 지정한 광고 타입의 로드 요청.
        internal static void RequestAd(AdType adType) {
            if (!CheckInitialized()) return;
            RequestAdInternal(adType);
        }

        private static void RequestAdInternal(AdType adType) {
            if (!TryGetProviderType(adType, out AdProviderType providerType)) return;

            AdUnitBase unit = GetAdUnit(providerType, adType);
            if (unit == null || unit.IsLoaded) return;

            LoadAd(providerType, unit).Forget();
        }
        
        // 광고 로드 수행.
        private static async UniTaskVoid LoadAd(AdProviderType providerType, AdUnitBase unit) {
            if (unit == null) return;

            if (Settings.Runtime.UseNetworkValidation) {
                bool connected = await Online.ValidateAsync(OnlineValidationType.All);
                if (!connected) return;
            }

            unit.Load();
            OnAdLoadRequested?.Invoke(providerType, unit.AdType);
        }

        
        // 지정한 광고 타입을 제공하는 광고 제공자 타입을 찾음.
        private static bool TryGetProviderType(AdType adType, out AdProviderType providerType) {
            providerType = AdProviderType.None;
            if (Settings == null || Settings.Providers == null) return false;

            providerType = adType switch {
                AdType.AppOpen => Settings.Providers.AppOpen,
                AdType.Banner => Settings.Providers.Banner,
                AdType.Interstitial => Settings.Providers.Interstitial,
                AdType.Rewarded => Settings.Providers.Rewarded,
                _ => AdProviderType.None
            };

            return providerType != AdProviderType.None;
        }
        
        // 지정한 광고 제공자에서 광고 유닛을 가져옴.
        private static AdUnitBase GetAdUnit(AdProviderType providerType, AdType adType) {
            if (!Providers.TryGetValue(providerType, out IAdsProvider provider) || provider == null) return null;
            if (!provider.IsInitialized) return null;
            if (!provider.Supports(adType)) return null;
            return provider.GetAdUnit(adType);
        }
        
        // 지정한 광고 타입에 대해 광고 제공자와 광고 유닛을 찾음.
        private static bool TryGetAdUnit(AdType adType, out AdProviderType providerType, out AdUnitBase unit) {
            unit = null;
            if (!TryGetProviderType(adType, out providerType)) return false;
            unit = GetAdUnit(providerType, adType);
            return unit != null;
        }
        
        // 지정한 광고 제공자가 현재 설정에서 사용 중인지 확인.
        private static bool IsProviderUsed(AdProviderType providerType) {
            if (Settings == null || Settings.Providers == null) return false;

            return Settings.Providers.AppOpen == providerType
                   || Settings.Providers.Banner == providerType
                   || Settings.Providers.Interstitial == providerType
                   || Settings.Providers.Rewarded == providerType;
        }
        
        #endregion

    }
}