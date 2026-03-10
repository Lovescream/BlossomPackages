#if SDK_APPLOVINMAX

using System;
using System.Collections.Generic;
using System.Linq;
using Blossom.Common;
using UnityEngine;

namespace Blossom.Platform.AppLovinMax {
    public class AppLovinMaxSettingsSO : SettingsSOBase {

        #region Properties

        public bool IsGdprEnabled => isGdprEnabled;
        public bool IsVerboseLog => isVerboseLog;

        public string[] TestDeviceIDs => testDevices?
            .Where(x => x != null && !string.IsNullOrWhiteSpace(x.DeviceId))
            .Select(x => x.DeviceId.Trim())
            .Distinct().ToArray() ?? Array.Empty<string>();

        #endregion

        #region Fields

        [SerializeField] private bool isGdprEnabled;
        [SerializeField] private bool isVerboseLog;

        [SerializeField] private TextAsset testDeviceCSV;
        [SerializeField] private List<AppLovinMaxTestDeviceEntry> testDevices = new();

        #endregion

        #region Editor

        public TextAsset TestDeviceCSV {
            get => testDeviceCSV;
            set => testDeviceCSV = value;
        }

        public List<AppLovinMaxTestDeviceEntry> TestDevices => testDevices;

        #endregion

    }

    [Serializable]
    public sealed class AppLovinMaxTestDeviceEntry {
        public string Owner;
        public string DeviceId;
    }
}

#endif