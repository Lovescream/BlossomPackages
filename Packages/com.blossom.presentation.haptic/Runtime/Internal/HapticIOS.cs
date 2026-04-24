namespace Blossom.Presentation.Haptic.Internal {
    using System.Runtime.InteropServices;
    using UnityEngine;

    internal static class HapticIOS {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _PlayHaptic(string type);
#endif

        public static void Play(HapticType type) {
#if UNITY_IOS && !UNITY_EDITOR
            _PlayHaptic(type.ToString());
#else
            Handheld.Vibrate();
#endif
        }
    }
}