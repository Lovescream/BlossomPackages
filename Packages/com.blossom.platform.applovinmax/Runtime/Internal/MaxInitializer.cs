#if SDK_APPLOVINMAX

using System;
using System.Collections.Generic;
using System.Linq;
using Blossom.Common;
using UnityEngine;

namespace Blossom.Platform.AppLovinMax.Internal {
    internal static class MaxInitializer {

        #region Const.

        private const string DisableLogsParameterName = "disable_all_logs";
        private const string DisableLogsParameterValue = "true";

        #endregion

        #region Properties

        internal static bool IsInitialized { get; private set; }
        internal static bool IsInitializing { get; private set; }

        #endregion

        #region Fields

        private static AppLovinMaxSettingsSO _settings;
        private static bool _callbackRegistered;
        private static readonly List<Action> PendingCallbacks = new();

        #endregion

        #region Initialize

        internal static void Initialize(AppLovinMaxSettingsSO settings = null, Action cbOnInitializeComplete = null) {
            if (IsInitialized) {
                cbOnInitializeComplete?.Invoke();
                return;
            }

            if (cbOnInitializeComplete != null) {
                PendingCallbacks.Add(cbOnInitializeComplete);
            }

            if (IsInitializing) return;

            _settings = settings ?? SettingsLoader.Get<AppLovinMaxSettingsSO>();
            if (_settings == null) {
                Debug.LogError($"[Blossom:AppLovinMax] Initialize(): SettingsSO not found.");
                FlushCallbacks();
                return;
            }

            IsInitializing = true;
            RegisterSdkCallback();
            try {
                ApplySettings(_settings);
                MaxSdk.InitializeSdk();
            }
            catch (Exception e) {
                IsInitializing = false;
                UnregisterSdkCallback();
                Debug.LogException(e);
                FlushCallbacks();
            }
        }

        private static void RegisterSdkCallback() {
            if (_callbackRegistered) return;
            MaxSdkCallbacks.OnSdkInitializedEvent += OnSdkInitialized;
            _callbackRegistered = true;
        }

        private static void UnregisterSdkCallback() {
            if (!_callbackRegistered) return;
            MaxSdkCallbacks.OnSdkInitializedEvent -= OnSdkInitialized;
            _callbackRegistered = false;
        }

        private static void ApplySettings(AppLovinMaxSettingsSO settings) {
            MaxSdk.SetVerboseLogging(settings.IsVerboseLog);
            if (!settings.IsVerboseLog) MaxSdk.SetExtraParameter(DisableLogsParameterName, DisableLogsParameterValue);

            string[] testDeviceIDs = settings.TestDeviceIDs
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct().ToArray();
            if (testDeviceIDs.Length > 0) MaxSdk.SetTestDeviceAdvertisingIdentifiers(testDeviceIDs);
        }

        private static void OnSdkInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration) {
            try {
                if (_settings != null && _settings.IsGdprEnabled) InitializeGdpr(sdkConfiguration);
                IsInitialized = true;
            }
            finally {
                IsInitializing = false;
                UnregisterSdkCallback();
                FlushCallbacks();
            }
        }
        
        private static void InitializeGdpr(MaxSdkBase.SdkConfiguration sdkConfiguration) {
            // 유저가 GDPR 적용 지역에 있는지 검사.
            if (sdkConfiguration.ConsentFlowUserGeography != MaxSdkBase.ConsentFlowUserGeography.Gdpr) return;

            MaxCmpService cmpService = MaxSdk.CmpService;
            cmpService.ShowCmpForExistingUser(error => {
                if (error == null) Nyo.Log("The CMP alert was shown successfully.");
                else Nyo.Error("Failed to show CMP alert.");
            });
        }
        
        #endregion

        private static void FlushCallbacks() {
            if (PendingCallbacks.Count == 0) return;
            Action[] callbacks = PendingCallbacks.ToArray();
            PendingCallbacks.Clear();

            foreach (Action cb in callbacks) {
                try {
                    cb?.Invoke();
                }
                catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }

    }
}

#endif