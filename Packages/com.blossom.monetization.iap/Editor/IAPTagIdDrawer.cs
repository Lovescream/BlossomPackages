#if UNITY_EDITOR
namespace Blossom.Monetization.IAP.Editor {
    using System;
    using System.Collections.Generic;
    using Blossom.Common.Editor;
    using UnityEditor;
    using UnityEngine;

        [CustomPropertyDrawer(typeof(IAPTagId))]
    public sealed class IAPTagIdDrawer : PropertyDrawer {

        private const double CacheRefreshInterval = 1.0;

        private static IAPSettingsSO _settings;
        private static string[] _options;
        private static Dictionary<string, int> _guidToIndex;
        private static bool _initialized;
        private static double _lastCacheTime;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (!_initialized || EditorApplication.timeSinceStartup - _lastCacheTime > CacheRefreshInterval) {
                InitializeCache();
            }

            SerializedProperty guidProperty = property.FindPropertyRelative("guid");
            if (guidProperty == null) {
                EditorGUI.LabelField(position, label.text, "guid field not found.");
                return;
            }

            if (_options == null || _options.Length <= 1) {
                EditorGUI.BeginProperty(position, label, property);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(position, label.text, "(등록된 태그 없음)");
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndProperty();
                return;
            }

            string guid = guidProperty.stringValue;
            int index = 0;
            if (!string.IsNullOrEmpty(guid) && _guidToIndex.TryGetValue(guid, out int cachedIndex)) {
                index = cachedIndex;
            }

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();

            int nextIndex = EditorGUI.Popup(position, label.text, index, _options);

            if (EditorGUI.EndChangeCheck()) {
                if (nextIndex <= 0) {
                    guidProperty.stringValue = string.Empty;
                }
                else {
                    IAPTagDefinition definition = _settings.TagDefinitions[nextIndex - 1];
                    guidProperty.stringValue = definition != null ? definition.Guid : string.Empty;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        private static void InitializeCache() {
            _settings = SettingsAssetUtility.FindSettingsAsset<IAPSettingsSO>();

            List<string> options = new() { "(None)" };
            _guidToIndex = new Dictionary<string, int>(StringComparer.Ordinal);

            if (_settings != null && _settings.TagDefinitions != null) {
                int index = 1;
                for (int i = 0; i < _settings.TagDefinitions.Count; i++) {
                    IAPTagDefinition definition = _settings.TagDefinitions[i];
                    if (definition == null) continue;
                    if (string.IsNullOrWhiteSpace(definition.Guid)) continue;

                    string name = string.IsNullOrWhiteSpace(definition.Name)
                        ? $"(Unnamed Tag {index})"
                        : definition.Name;

                    options.Add(name);
                    _guidToIndex[definition.Guid] = index;
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