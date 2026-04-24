namespace Blossom.Presentation.Haptic {
    using Blossom.Presentation.Haptic.Internal;
    using UnityEngine;
    
    public static class Haptic {
        public static bool Enabled { get; set; } = true;

        public static void Play(HapticType type = HapticType.Light) {
            if (!Enabled) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            HapticAndroid.Play(type);
#elif UNITY_IOS && !UNITY_EDITOR
            HapticIOS.Play(type);
#else
            Handheld.Vibrate();
#endif
        }
    }

    public enum HapticType {
        Light, Medium, Heavy, Success, Warning, Error
    }
}