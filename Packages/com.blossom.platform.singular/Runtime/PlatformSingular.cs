#if SDK_SINGULAR

using System;
using Blossom.Platform.Singular.Internal;

namespace Blossom.Platform.Singular {
    public static class PlatformSingular {
        public static bool IsInitialized => SingularInitializer.IsInitialized;

        public static void Initialize(SingularSettingsSO settings = null, Action onComplete = null) =>
            SingularInitializer.Initialize(settings, onComplete);
        
        /// <summary>
        /// CustomId를 설정.
        /// </summary>
        /// <param name="id">설정할 커스텀 Id.</param>
        public static void SetCustomUserId(string id) => SingularController.SetCustomUserId(id);
    }
}

#endif