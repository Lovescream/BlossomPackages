#if UNITY_EDITOR

namespace Blossom.Monetization.IAP.Editor {
    using System;
    using System.Collections.Generic;
    using Blossom.Common.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomPropertyDrawer(typeof(IAPTagAttribute))]
    public sealed class IAPTagNameAttributeDrawer : PropertyDrawer {

        private const double CacheRefreshInterval = 5.0;

        private static IAPSettingsSO _settings;
        private static string[] _options;
        private static Dictionary<string, int> _tagNameIndexes;
        private static bool _initialized;
        private static double _lastCacheTime;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.String) {
                EditorGUI.LabelField(position, label.text, "IAPTagNameAttribute can only be used on string fields.");
                return;
            }

            if (!_initialized || EditorApplication.timeSinceStartup - _lastCacheTime > CacheRefreshInterval) {
                InitializeCache();
            }

            if (_options == null || _options.Length <= 1) {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string value = property.stringValue;
            int index = 0;

            if (!string.IsNullOrEmpty(value) && _tagNameIndexes.TryGetValue(value, out int cachedIndex)) {
                index = cachedIndex;
            }

            EditorGUI.BeginChangeCheck();

            int nextIndex = EditorGUI.Popup(position, label.text, index, _options);

            if (EditorGUI.EndChangeCheck()) {
                property.stringValue = nextIndex <= 0 ? string.Empty : _options[nextIndex];
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        private static void InitializeCache() {
            _settings = SettingsAssetUtility.FindSettingsAsset<IAPSettingsSO>();

            List<string> options = new() { "(None)" };
            _tagNameIndexes = new Dictionary<string, int>(StringComparer.Ordinal);

            if (_settings != null && _settings.TagDefinitions != null) {
                int index = 1;

                for (int i = 0; i < _settings.TagDefinitions.Count; i++) {
                    IAPTagDefinition definition = _settings.TagDefinitions[i];
                    if (definition == null) continue;
                    if (string.IsNullOrWhiteSpace(definition.Name)) continue;
                    if (_tagNameIndexes.ContainsKey(definition.Name)) continue;

                    options.Add(definition.Name);
                    _tagNameIndexes.Add(definition.Name, index);
                    index++;
                }
            }

            _options = options.ToArray();
            _initialized = true;
            _lastCacheTime = EditorApplication.timeSinceStartup;
        }

    }
}

#endif