#if SDK_APPLOVINMAX

using System;
using UnityEngine;

namespace Blossom.Monetization.Ads.Internal {
    internal sealed class MaxBannerAdUnit : AdUnitBase {
        
        #region Properties

        public override AdType AdType => AdType.Banner;

        #endregion

        #region Fields

        private readonly string _adUnitId;

        private bool _eventsRegistered;
        private bool _isCreated;

        #endregion

        #region Constructor

        internal MaxBannerAdUnit(AdsSettingsSO settings) {
            _adUnitId = settings?.AppLovin?.BannerID ?? string.Empty;
        }

        #endregion

        #region Ads

        public override void Load() {
            if (string.IsNullOrEmpty(_adUnitId)) {
                Debug.LogError($"[Blossom:Ads:MaxBannerAdUnit] Load(): No ad unit ID specified.");
                return;
            }

            if (_isCreated) return;
            
            RegisterEvents();

            MaxSdk.CreateBanner(_adUnitId, new MaxSdkBase.AdViewConfiguration(MaxSdkBase.AdViewPosition.BottomCenter));
            MaxSdk.SetBannerBackgroundColor(_adUnitId, Color.clear);
            
            _isCreated = true;
            IsLoaded = true;
            CallOnLoaded();
        }

        public override void Show(Action<bool> callback = null) {
            if (string.IsNullOrEmpty(_adUnitId)) {
                callback?.Invoke(false);
                return;
            }

            if (!_isCreated) {
                callback?.Invoke(false);
                return;
            }

            try {
                BeginShow(callback);
                MaxSdk.ShowBanner(_adUnitId);
                IsShowing = true;
                CallOnDisplayed();
                CompleteShow(true);
            }
            catch (Exception e) {
                Debug.LogError($"[Blossom:Ads:MaxBannerAdUnit] Show(): {e}");
                CompleteShow(false);
            }
        }

        public override void Hide() {
            if (!_isCreated) return;
            if (!IsShowing) return;
            
            MaxSdk.HideBanner(_adUnitId);
            IsShowing = false;
            CallOnClosed();
        }

        public override void Destroy() {
            if (!_isCreated) return;
            bool wasShowing = IsShowing;
            MaxSdk.DestroyBanner(_adUnitId);
            IsLoaded = false;
            IsShowing = false;
            _isCreated = false;
            UnregisterEvents();
            if (wasShowing) CallOnClosed();
        }
        
        #endregion

        #region Events

        private void RegisterEvents() {
            if (_eventsRegistered) return;

            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;
            
            _eventsRegistered = true;
        }

        private void UnregisterEvents() {
            if (!_eventsRegistered) return;

            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;
            
            _eventsRegistered = false;
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (adUnitId != _adUnitId) return;

            AdRevenueInfo info = new(AdProviderType.AppLovin, AdType.Banner, adUnitId, adInfo.NetworkName,
                adInfo.Revenue, "USD");

            CallOnRevenuePaid(info);
        }

        #endregion
        
    }
}

#endif