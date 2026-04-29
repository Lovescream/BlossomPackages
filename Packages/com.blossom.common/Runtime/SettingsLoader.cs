namespace Blossom.Common {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    
    public static class SettingsLoader {

        private const string SettingsResourcePath = "@Blossom/Settings/";

        private static Dictionary<Type, SettingsSOBase> _settings;

        private static bool _isInitialized;

        public static T Get<T>() where T : SettingsSOBase {
            Initialize();
            if (_settings.TryGetValue(typeof(T), out SettingsSOBase setting)) return setting as T;
            return null;
        }

        private static void Initialize() {
            if (_isInitialized) return;

            _settings = Resources.LoadAll<SettingsSOBase>(SettingsResourcePath).ToDictionary(x => x.GetType(), x => x);
            
            _isInitialized = true;
        }

    }

    public abstract class SettingsSOBase : ScriptableObject { } 
}