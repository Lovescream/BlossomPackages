#if UNITY_EDITOR && SDK_APPLOVINMAX

namespace Blossom.Platform.AppLovinMax.Editor {
    using System;
    using System.Collections.Generic;
    using Blossom.Common.Editor;
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(AppLovinMaxSettingsSO))]
    public sealed class AppLovinMaxSettingsSOEditor : UnityEditor.Editor {

        #region Const.

        private const string AssetFileName = "AppLovinMaxSettings.asset";
        private const string SettingsMenuPath = "Blossom/Settings/Platform/AppLovinMaxSettings";
        private const string CreateMenuPath = "Assets/Create/@Blossom/Platform/AppLovinMax Settings";

        #endregion

        #region Fields

        private SerializedProperty _isVerboseLogProperty;
        private SerializedProperty _isInitGdprProperty;
        private SerializedProperty _testDeviceCsvProperty;
        private SerializedProperty _testDevicesProperty;

        private bool _showCsvInfo;

        #endregion

        #region Initialize
  
        private void OnEnable() {
            _isVerboseLogProperty = serializedObject.FindProperty("isVerboseLog");
            _isInitGdprProperty = serializedObject.FindProperty("isGdprEnabled");
            _testDeviceCsvProperty = serializedObject.FindProperty("testDeviceCSV");
            _testDevicesProperty = serializedObject.FindProperty("testDevices");
        }

        #endregion

        #region Inspector

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_isVerboseLogProperty);
            EditorGUILayout.PropertyField(_isInitGdprProperty);

            EditorGUILayout.Space();

            _showCsvInfo = EditorGUILayout.Foldout(_showCsvInfo, "CSV 매핑 정보");
            if (_showCsvInfo) {
                EditorGUILayout.HelpBox(
                    "CSV 파일 형식:\n" +
                    "1. 첫 번째 줄은 헤더 (예: Owner,TestId)\n" +
                    "2. 두 번째 줄부터 데이터\n" +
                    "3. 각 줄은 쉼표(,)로 구분\n" +
                    "4. 예시:\n" +
                    "Owner,TestId\n" +
                    "Sejin,abcd1234\n" +
                    "Tester,efgh5678",
                    MessageType.Info
                );
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_testDeviceCsvProperty);

            if (_testDeviceCsvProperty.objectReferenceValue != null) {
                if (GUILayout.Button("CSV에서 테스트 기기 매핑하기", GUILayout.Height(30))) {
                    MapCsvToList();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_testDevicesProperty, true);

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region CSV Mapping

        private void MapCsvToList() {
            AppLovinMaxSettingsSO settings = (AppLovinMaxSettingsSO)target;
            TextAsset csvFile = settings.TestDeviceCSV;

            if (csvFile == null) {
                EditorUtility.DisplayDialog("오류", "CSV 파일이 유효하지 않습니다.", "확인");
                return;
            }

            try {
                string csvText = csvFile.text;
                string[] lines = csvText.Split('\n');

                if (lines.Length < 2) {
                    EditorUtility.DisplayDialog("오류", "CSV 파일이 비어있거나 헤더만 있습니다.", "확인");
                    return;
                }

                List<AppLovinMaxTestDeviceEntry> entries = new();

                for (int i = 1; i < lines.Length; i++) {
                    string line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line)) continue;

                    string[] values = line.Split(',');
                    if (values.Length < 2) continue;

                    string owner = values[0].Trim();
                    string deviceId = values[1].Trim();

                    if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(deviceId)) continue;

                    entries.Add(new AppLovinMaxTestDeviceEntry {
                        Owner = owner,
                        DeviceId = deviceId
                    });
                }

                if (entries.Count == 0) {
                    EditorUtility.DisplayDialog("결과", "매핑할 유효한 엔트리가 없습니다.", "확인");
                    return;
                }

                Undo.RecordObject(settings, "Map Test Devices From CSV");

                settings.TestDevices.Clear();
                settings.TestDevices.AddRange(entries);

                EditorUtility.SetDirty(settings);
                serializedObject.Update();

                EditorUtility.DisplayDialog("성공", $"{entries.Count}개의 테스트 기기가 매핑되었습니다.", "확인");
            }
            catch (Exception e) {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("오류", $"CSV 매핑 중 오류가 발생했습니다.\n{e.Message}", "확인");
            }
        }

        #endregion

        #region Menu

        [MenuItem(SettingsMenuPath)]
        private static void SelectOrCreateSettings() {
            SettingsAssetUtility.SelectOrCreateAtRoot<AppLovinMaxSettingsSO>(AssetFileName);
        }

        [MenuItem(CreateMenuPath)]
        private static void CreateSettingsAsset() {
            SettingsAssetUtility.SelectOrCreateAtRoot<AppLovinMaxSettingsSO>(AssetFileName);
        }

        #endregion
    }
}

#endif