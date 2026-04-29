#if SDK_APPLOVINMAX

namespace Blossom.Platform.AppLovinMax {
    using System;
    using Internal;
    
    public static class PlatformAppLovinMax {

        public static bool IsInitialized => MaxInitializer.IsInitialized;
        
        public static bool IsInitializing => MaxInitializer.IsInitializing;

        public static void Initialize(AppLovinMaxSettingsSO settings = null, Action cbOnInitializeComplete = null)
            => MaxInitializer.Initialize(settings, cbOnInitializeComplete);

    }
}

#endif