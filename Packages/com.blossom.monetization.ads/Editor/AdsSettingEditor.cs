#if UNITY_EDITOR

namespace Blossom.Monetization.Ads.Editor {
    using Blossom.Common.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AdsSettingsSO))]
    internal sealed class AdsSettingEditor : Editor {

        #region Const.

        private const string AssetFileName = "AdsSettings.asset";
        private const string SettingsMenuPath = "Blossom/Settings/Monetization/AdsSettings";
        private const string CreateMenuPath = "Assets/Create/@Blossom/Monetization/Ads Settings";

        #endregion

        #region Fields

        private SerializedProperty _providersProperty;
        private SerializedProperty _runtimeProperty;
        private SerializedProperty _appLovinProperty;
        private SerializedProperty _adMobProperty;

        private GUIStyle _headerStyle;
        private GUIStyle _boxStyle;

        #endregion

        #region Initialize

        private void OnEnable() {
            _providersProperty = serializedObject.FindProperty("providers");
            _runtimeProperty = serializedObject.FindProperty("runtime");
            _appLovinProperty = serializedObject.FindProperty("appLovin");
            _adMobProperty = serializedObject.FindProperty("adMob");

            InitializeStyles();
        }

        private void InitializeStyles() {
            _headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            _boxStyle = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(10, 10, 8, 8),
                margin = new RectOffset(0, 0, 6, 6)
            };
        }

        #endregion

        #region Inspector

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawProvidersSection();
            DrawRuntimeSection();
            DrawAppLovinSection();
            DrawAdMobSection();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Draw Sections

        private void DrawProvidersSection() {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("Providers", _headerStyle);
            EditorGUILayout.Space(2f);

            EditorGUILayout.PropertyField(_providersProperty, true);
            DrawProviderSummaryHelpBox();

            EditorGUILayout.EndVertical();
        }

        private void DrawRuntimeSection() {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("Runtime", _headerStyle);
            EditorGUILayout.Space(2f);

            EditorGUILayout.PropertyField(_runtimeProperty, true);

            EditorGUILayout.EndVertical();
        }

        private void DrawAppLovinSection() {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("AppLovin", _headerStyle);
            EditorGUILayout.Space(2f);

            EditorGUILayout.PropertyField(_appLovinProperty, true);

            EditorGUILayout.EndVertical();
        }

        private void DrawAdMobSection() {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField("AdMob", _headerStyle);
            EditorGUILayout.Space(2f);

            EditorGUILayout.PropertyField(_adMobProperty, true);

            EditorGUILayout.EndVertical();
        }

        private void DrawProviderSummaryHelpBox() {
            if (_providersProperty == null) return;

            AdProviderType appOpen = (AdProviderType)_providersProperty.FindPropertyRelative("appOpen").enumValueIndex;
            AdProviderType banner = (AdProviderType)_providersProperty.FindPropertyRelative("banner").enumValueIndex;
            AdProviderType interstitial =
                (AdProviderType)_providersProperty.FindPropertyRelative("interstitial").enumValueIndex;
            AdProviderType rewarded =
                (AdProviderType)_providersProperty.FindPropertyRelative("rewarded").enumValueIndex;

            string summary =
                $"AppOpen: {appOpen}\n" +
                $"Banner: {banner}\n" +
                $"Interstitial: {interstitial}\n" +
                $"Rewarded: {rewarded}";

            EditorGUILayout.HelpBox(summary, MessageType.None);
        }

        #endregion

        #region Menu

        [MenuItem(SettingsMenuPath)]
        private static void SelectOrCreateSettings() {
            SettingsAssetUtility.SelectOrCreateAtRoot<AdsSettingsSO>(AssetFileName);
        }

        [MenuItem(CreateMenuPath)]
        private static void CreateSettingsAsset() {
            SettingsAssetUtility.SelectOrCreateAtRoot<AdsSettingsSO>(AssetFileName);
        }

        #endregion

    }
}

#endif