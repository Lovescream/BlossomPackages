#if SDK_APPLOVINMAX

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Blossom.Monetization.Ads.Internal {
    internal sealed class MaxInterstitialAdUnit : AdUnitBase {

        #region Properties

        public override AdType AdType => AdType.Interstitial;

        #endregion

        #region Fields

        private readonly AdsSettingsSO _settings;
        private readonly string _adUnitId;

        private bool _eventsRegistered;
        private bool _isLoading;
        private int _loadAttempt;

        private CancellationTokenSource _retryCts;

        #endregion

        #region Constructor

        internal MaxInterstitialAdUnit(AdsSettingsSO settings) {
            _settings = settings;
            _adUnitId = settings?.AppLovin?.InterstitialID ?? string.Empty;
        }

        #endregion

        #region Ads

        public override void Load() {
            if (string.IsNullOrEmpty(_adUnitId)) {
                Debug.LogError($"[Blossom:Ads:MaxInterstitialAdUnit] Load(): No ad unit ID specified.");
                return;
            }

            if (_isLoading || IsLoaded) return;

            RegisterEvents();
            CancelRetry();

            _isLoading = true;
            MaxSdk.LoadInterstitial(_adUnitId);
        }

        public override void Show(Action<bool> callback = null) {
            if (string.IsNullOrEmpty(_adUnitId)) {
                callback?.Invoke(false);
                return;
            }

            if (!IsLoaded || IsShowing) {
                callback?.Invoke(false);
                return;
            }

            if (!MaxSdk.IsInterstitialReady(_adUnitId)) {
                IsLoaded = false;
                callback?.Invoke(false);
                return;
            }

            BeginShow(callback);

            try {
                MaxSdk.ShowInterstitial(_adUnitId);
            }
            catch (Exception e) {
                Debug.LogError($"[Blossom:Ads:MaxInterstitialAdUnit] Show(): {e}");
                CompleteShow(false);
            }
        }

        public override void Destroy() {
            CancelRetry();
            UnregisterEvents();
            
            IsLoaded = false;
            IsShowing = false;
            _isLoading = false;
            _loadAttempt = 0;
        }
        
        #endregion

        #region Events

        private void RegisterEvents() {
            if (_eventsRegistered) return;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidEvent;

            _eventsRegistered = true;
        }

        private void UnregisterEvents() {
            if (!_eventsRegistered) return;

            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= OnAdLoadedEvent;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= OnAdLoadFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent -= OnAdDisplayedEvent;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= OnAdHiddenEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= OnAdDisplayFailedEvent;
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= OnAdRevenuePaidEvent;

            _eventsRegistered = false;
        }

        private void OnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (adUnitId != _adUnitId) return;

            _isLoading = false;
            _loadAttempt = 0;
            IsLoaded = true;
            
            CallOnLoaded();
        }

        private void OnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo) {
            if (adUnitId != _adUnitId) return;

            _isLoading = false;
            IsLoaded = false;

            RetryLoadWithBackoff().Forget();
        }

        private void OnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (adUnitId != _adUnitId) return;

            IsShowing = true;
            CallOnDisplayed();
        }

        private void OnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (adUnitId != _adUnitId) return;

            IsLoaded = false;
            IsShowing = false;

            CompleteShow(true);
            CallOnClosed();
        }

        private void OnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo) {
            if (adUnitId != _adUnitId) return;
            
            IsLoaded = false;
            IsShowing = false;

            CompleteShow(false);
            CallOnClosed();

            Load();
        }

        private void OnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) {
            if (adUnitId != _adUnitId) return;
            AdRevenueInfo info = new(AdProviderType.AppLovin, AdType.Interstitial, adUnitId, adInfo.NetworkName,
                adInfo.Revenue, "USD");
            CallOnRevenuePaid(info);
        }

        #endregion

        #region Retry

        private async UniTask RetryLoadWithBackoff() {
            int maxAttempts = Mathf.Max(0, _settings?.Runtime?.LoadMaxAttempts ?? 0);
            if (maxAttempts <= 0) return;

            _loadAttempt++;
            if (_loadAttempt > maxAttempts) return;

            CancelRetry();
            _retryCts = new CancellationTokenSource();

            try {
                float delaySeconds = Mathf.Pow(2f, _loadAttempt - 1);
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: _retryCts.Token);

                if (_retryCts == null || _retryCts.IsCancellationRequested) return;

                Load();
            }
            catch (OperationCanceledException) {
                // 의도된 취소.
            }
        }

        private void CancelRetry() {
            if (_retryCts == null) return;

            try {
                _retryCts.Cancel();
                _retryCts.Dispose();
            }
            catch {
                // 취소/해제 중 예외는 무시.
            }
            finally {
                _retryCts = null;
            }
        }
        
        #endregion

    }
}

#endif