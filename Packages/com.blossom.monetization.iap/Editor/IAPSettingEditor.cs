#if UNITY_EDITOR

namespace Blossom.Monetization.IAP.Editor {
    using System;
    using System.Collections.Generic;
    using Blossom.Common.Editor;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.Purchasing;

    [CustomEditor(typeof(IAPSettingsSO))]
    public class IAPSettingEditor : UnityEditor.Editor {

        #region Const.

        private const string AssetFileName = "IAPSettings.asset";
        private const string SettingsMenuPath = "Blossom/Settings/Monetization/IAPSettings";
        private const string CreateMenuPath = "Assets/Create/@Blossom/Monetization/IAP Settings";

        #endregion

        #region Fields

        private bool _showAddProductUI;
        private string _newProductKey;
        private string _errorMessage;

        private readonly Dictionary<int, bool> _foldoutStates = new();
        private readonly Dictionary<string, bool> _duplicateKeys = new();

        private GUIStyle _folderHeaderStyle;
        private GUIStyle _foldoutStyle;
        private GUIStyle _headerLabelStyle;

        private SerializedProperty _definitionProperty;
        private SerializedProperty _tagDefinitionProperty;

        private ReorderableList _tagRegistryList;
        private readonly Dictionary<string, ReorderableList> _productTagsLists = new();

        #endregion

        #region Initialize

        private void OnEnable() {
            _definitionProperty = serializedObject.FindProperty("definitions");
            _tagDefinitionProperty = serializedObject.FindProperty("tagDefinitions");
            _foldoutStates.Clear();
            _productTagsLists.Clear();
            CheckDuplicateKeys();
            InitializeStyles();
            BuildTagRegistryList();
        }

        private void CheckDuplicateKeys() {
            _duplicateKeys.Clear();
            if (!(_definitionProperty?.arraySize > 0)) return;

            Dictionary<string, int> keyCount = new();
            for (int i = 0; i < _definitionProperty.arraySize; i++) {
                SerializedProperty property = _definitionProperty.GetArrayElementAtIndex(i);
                string key = property.FindPropertyRelative("key").stringValue;
                if (string.IsNullOrEmpty(key)) continue;

                if (keyCount.TryAdd(key, 1)) continue;
                keyCount[key]++;
                _duplicateKeys[key] = true;
            }
        }

        private void InitializeStyles() {
            _folderHeaderStyle = new GUIStyle(EditorStyles.helpBox) {
                padding = new RectOffset(6, 6, 6, 6),
                margin = new RectOffset(0, 0, 2, 2)
            };
            _foldoutStyle = new GUIStyle(EditorStyles.foldout) {
                fontStyle = FontStyle.Bold
            };
            _headerLabelStyle = new GUIStyle(EditorStyles.boldLabel) {
                normal = { textColor = Color.white },
                margin = new RectOffset(0, 0, 2, 0)
            };
        }

        private void BuildTagRegistryList() {
            if (_tagDefinitionProperty == null) return;

            _tagRegistryList = new ReorderableList(serializedObject, _tagDefinitionProperty, true, true, true, true);
            _tagRegistryList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "IAP Tags"); };
            _tagRegistryList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 10;
            _tagRegistryList.drawElementCallback = (rect, index, isActive, isFocused) => {
                SerializedProperty element = _tagDefinitionProperty.GetArrayElementAtIndex(index);
                SerializedProperty guidProperty = element.FindPropertyRelative("guid");
                SerializedProperty keyProperty = element.FindPropertyRelative("key");

                rect.y += 2;
                Rect guidRect = new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);
                Rect keyRect = new(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 4, rect.width,
                    EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(guidRect, guidProperty);
                EditorGUI.PropertyField(keyRect, keyProperty);
            };
        }

        #endregion

        #region Inspector

        public override void OnInspectorGUI() {
            serializedObject.Update();

            DrawTagRegistrySection();
            EditorGUILayout.Space(8f);
            DrawProductsSection();
            EditorGUILayout.Space(8f);
            DrawAddProductSection();
            EditorGUILayout.Space(8f);
            DrawCleanupSection();

            serializedObject.ApplyModifiedProperties();

            CheckDuplicateKeys();
        }

        private void DrawTagRegistrySection() {
            EditorGUILayout.BeginVertical(_folderHeaderStyle);
            EditorGUILayout.LabelField("Tag Definitions", _headerLabelStyle);

            _tagRegistryList?.DoLayoutList();

            EditorGUILayout.EndVertical();
        }

        private void DrawProductsSection() {
            EditorGUILayout.BeginVertical(_folderHeaderStyle);
            EditorGUILayout.LabelField("Products", _headerLabelStyle);

            if (_definitionProperty == null || _definitionProperty.arraySize == 0) {
                EditorGUILayout.HelpBox("등록된 상품이 없습니다.", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            for (int i = 0; i < _definitionProperty.arraySize; i++) {
                SerializedProperty element = _definitionProperty.GetArrayElementAtIndex(i);
                DrawProductElement(element, i);
                EditorGUILayout.Space(4f);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawProductElement(SerializedProperty element, int index) {
            SerializedProperty keyProperty = element.FindPropertyRelative("key");
            string key = keyProperty.stringValue;
            string title = string.IsNullOrEmpty(key) ? $"Product {index}" : key;

            bool isOpen = _foldoutStates.TryGetValue(index, out bool opened) && opened;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            _foldoutStates[index] = EditorGUILayout.Foldout(isOpen, title, true, _foldoutStyle);

            if (_duplicateKeys.TryGetValue(key, out bool isDuplicate) && isDuplicate) {
                EditorGUILayout.HelpBox($"중복된 Key입니다: {key}", MessageType.Error);
            }

            if (_foldoutStates[index]) {
                EditorGUILayout.PropertyField(element, true);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("삭제", GUILayout.Width(80))) {
                    _definitionProperty.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawAddProductSection() {
            EditorGUILayout.BeginVertical(_folderHeaderStyle);
            EditorGUILayout.LabelField("Add Product", _headerLabelStyle);

            _showAddProductUI = EditorGUILayout.Foldout(_showAddProductUI, "새 상품 추가", true);
            if (_showAddProductUI) {
                _newProductKey = EditorGUILayout.TextField("Key", _newProductKey);

                if (!string.IsNullOrEmpty(_errorMessage)) {
                    EditorGUILayout.HelpBox(_errorMessage, MessageType.Error);
                }

                if (GUILayout.Button("상품 추가", GUILayout.Height(28))) {
                    TryAddProduct();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawCleanupSection() {
            EditorGUILayout.BeginVertical(_folderHeaderStyle);
            EditorGUILayout.LabelField("Maintenance", _headerLabelStyle);

            if (GUILayout.Button("유효하지 않은 태그 참조 정리")) {
                int removed = CleanupInvalidTagReferences();
                EditorUtility.DisplayDialog("정리 완료", $"{removed}개의 잘못된 태그 참조를 제거했습니다.", "확인");
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Product Management

        private void TryAddProduct() {
            _errorMessage = null;

            if (string.IsNullOrWhiteSpace(_newProductKey)) {
                _errorMessage = "Key를 입력해주세요.";
                return;
            }

            for (int i = 0; i < _definitionProperty.arraySize; i++) {
                SerializedProperty element = _definitionProperty.GetArrayElementAtIndex(i);
                string existingKey = element.FindPropertyRelative("key").stringValue;
                if (string.Equals(existingKey, _newProductKey, StringComparison.Ordinal)) {
                    _errorMessage = "동일한 Key가 이미 존재합니다.";
                    return;
                }
            }

            int newIndex = _definitionProperty.arraySize;
            _definitionProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty newElement = _definitionProperty.GetArrayElementAtIndex(newIndex);
            newElement.FindPropertyRelative("key").stringValue = _newProductKey;
            newElement.FindPropertyRelative("aosID").stringValue = string.Empty;
            newElement.FindPropertyRelative("iosID").stringValue = string.Empty;
            newElement.FindPropertyRelative("type").enumValueIndex = (int)ProductType.Consumable;

            SerializedProperty tagsProperty = newElement.FindPropertyRelative("tags");
            if (tagsProperty != null) tagsProperty.arraySize = 0;

            _foldoutStates[newIndex] = true;
            _newProductKey = string.Empty;

            serializedObject.ApplyModifiedProperties();
        }

        private bool TagGuidExists(string guid) {
            if (string.IsNullOrEmpty(guid) || _tagDefinitionProperty == null) return false;

            for (int i = 0; i < _tagDefinitionProperty.arraySize; i++) {
                SerializedProperty element = _tagDefinitionProperty.GetArrayElementAtIndex(i);
                string existingGuid = element.FindPropertyRelative("guid").stringValue;
                if (string.Equals(existingGuid, guid, StringComparison.Ordinal)) return true;
            }

            return false;
        }

        private int CleanupInvalidTagReferences() {
            if (_definitionProperty == null) return 0;

            int removed = 0;
            for (int i = 0; i < _definitionProperty.arraySize; i++) {
                SerializedProperty definition = _definitionProperty.GetArrayElementAtIndex(i);
                SerializedProperty tags = definition.FindPropertyRelative("tags");
                if (tags == null || !tags.isArray) continue;

                for (int j = tags.arraySize - 1; j >= 0; j--) {
                    SerializedProperty element = tags.GetArrayElementAtIndex(j);
                    string guid = element.FindPropertyRelative("guid").stringValue;

                    if (string.IsNullOrEmpty(guid)) {
                        tags.DeleteArrayElementAtIndex(j);
                        removed++;
                        continue;
                    }

                    if (!TagGuidExists(guid)) {
                        tags.DeleteArrayElementAtIndex(j);
                        removed++;
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            return removed;
        }

        #endregion

        #region Menu

        [MenuItem(SettingsMenuPath)]
        public static void SelectOrCreateSettings() {
            SettingsAssetUtility.SelectOrCreateAtRoot<IAPSettingsSO>(AssetFileName);
        }

        [MenuItem(CreateMenuPath)]
        private static void CreateSettingsAsset() {
            SettingsAssetUtility.SelectOrCreateAtRoot<IAPSettingsSO>(AssetFileName);
        }

        #endregion

    }
}

#endif