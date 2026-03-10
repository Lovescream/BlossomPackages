#if SDK_APPLOVINMAX

using System;
using Blossom.Platform.AppLovinMax.Internal;

namespace Blossom.Platform.AppLovinMax {
    public static class PlatformAppLovinMax {

        public static bool IsInitialized => MaxInitializer.IsInitialized;
        
        public static bool IsInitializing => MaxInitializer.IsInitializing;

        public static void Initialize(AppLovinMaxSettingsSO settings = null, Action cbOnInitializeComplete = null)
            => MaxInitializer.Initialize(settings, cbOnInitializeComplete);

    }
}

#endif