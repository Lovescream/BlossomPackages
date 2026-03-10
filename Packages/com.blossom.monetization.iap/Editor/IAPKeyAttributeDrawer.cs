#if UNITY_EDITOR

namespace Blossom.Monetization.IAP.Editor {
    using System.Collections.Generic;
    using Blossom.Common.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(IAPKeyAttribute))]
    public class IAPKeyAttributeDrawer : PropertyDrawer {

        private const double CacheRefreshInterval = 5.0;

        private static IAPSettingsSO _settings;
        private static Dictionary<string, int> _keyIndexes;
        private static string[] _options;
        private static bool _initialized;
        private static double _lastCacheTime;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!_initialized || EditorApplication.timeSinceStartup - _lastCacheTime > CacheRefreshInterval)
                InitializeCache();

            if (_options == null || _options.Length <= 1) {
                EditorGUI.PropertyField(position, property, label);
                EditorGUILayout.HelpBox("설정 파일을 생성하거나 상품을 등록해야 한다데스", MessageType.Warning);
                return;
            }

            string value = property.stringValue;
            int index = 0;
            if (!string.IsNullOrEmpty(value) && _keyIndexes.TryGetValue(value, out int keyIndex)) index = keyIndex;

            EditorGUI.BeginChangeCheck();
            index = EditorGUI.Popup(position, label.text, index, _options);
            if (EditorGUI.EndChangeCheck()) property.stringValue = index == 0 ? "" : _options[index];
        }

        private void InitializeCache() {
            _settings = GetSettings();

            List<string> options = new() { "(None)" };
            _keyIndexes = new();

            if (_settings != null && _settings.Definitions != null) {
                int index = 1;
                foreach (IAPDefinition definition in _settings.Definitions) {
                    if (string.IsNullOrEmpty(definition.Key)) continue;
                    options.Add(definition.Key);
                    _keyIndexes[definition.Key] = index++;
                }
            }

            _options = options.ToArray();
            _initialized = true;
            _lastCacheTime = EditorApplication.timeSinceStartup;
        }

        private IAPSettingsSO GetSettings() {
            return SettingsAssetUtility.FindSettingsAsset<IAPSettingsSO>();
        }
    }
}

#endif