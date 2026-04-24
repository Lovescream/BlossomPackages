namespace Blossom.Presentation.Haptic.Internal {
    using UnityEngine;
    
    internal static class HapticAndroid {

        private static bool _initialized;

        private static AndroidJavaClass _unityPlayer;
        private static AndroidJavaObject _activity;
        private static AndroidJavaObject _vibrator;
        private static AndroidJavaClass _effector;
        private static int _androidVersion;
        private static bool _hasVibrator;
        private static bool _hasAmplitudeControl;

        internal static void Initialize() {
            if (_initialized) return;

            try {
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _activity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                if (_activity == null) return;
                
                _vibrator = _activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (_vibrator == null) return;
                
                _androidVersion = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
                _hasVibrator = _vibrator.Call<bool>("hasVibrator");

                if (_androidVersion >= 26) {
                    _effector = new AndroidJavaClass("android.os.VibrationEffect");
                    _hasAmplitudeControl = _vibrator.Call<bool>("hasAmplitudeControl");
                }
                _initialized = true;
            }
            catch {
                _initialized = false;
            }
        }
        
        internal static void Play(HapticType type) {
            Initialize();

            if (!_initialized || _vibrator == null || !_hasVibrator) return;
            
            long duration = type switch {
                HapticType.Light => 20,
                HapticType.Medium => 40,
                HapticType.Heavy => 70,
                HapticType.Success => 40,
                HapticType.Warning => 80,
                HapticType.Error => 120,
                _ => 30
            };

            int amplitude = type switch {
                HapticType.Light => 60,
                HapticType.Medium => 120,
                HapticType.Heavy => 255,
                HapticType.Success => 120,
                HapticType.Warning => 180,
                HapticType.Error => 255,
                _ => 100
            };

            try {
                if (_androidVersion >= 26 && _effector != null) {
                    using AndroidJavaObject effect =
                        _effector.CallStatic<AndroidJavaObject>("createOneShot", duration,
                            _hasAmplitudeControl ? amplitude : -1);
                    _vibrator.Call("vibrate", effect);
                }
                else _vibrator.Call("vibrate", duration);
            }
            catch {
                Handheld.Vibrate();
            }
        }
    }
}