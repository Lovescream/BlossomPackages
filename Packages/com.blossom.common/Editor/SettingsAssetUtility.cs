#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Blossom.Common.Editor {

    public static class SettingsAssetUtility {
        public const string RootFolderPath = "Assets/Resources/@Blossom/Settings";

        public static T SelectOrCreateAtRoot<T>(string assetFileName) where T : ScriptableObject {
            T existing = FindSettingsAsset<T>();
            if (existing != null) {
                SelectAndPing(existing);
                return existing;
            }

            return CreateAtRoot<T>(assetFileName);
        }

        public static T SelectExisting<T>() where T : ScriptableObject {
            T existing = FindSettingsAsset<T>();
            if (existing != null) SelectAndPing(existing);
            return existing;
        }

        public static T CreateAtRoot<T>(string assetFileName) where T : ScriptableObject {
            T existing = FindSettingsAsset<T>();
            if (existing != null) {
                SelectAndPing(existing);
                return existing;
            }

            EnsureFolderExists(RootFolderPath);

            string assetPath = Path.Combine(RootFolderPath, assetFileName).Replace("\\", "/");
            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = Path.GetFileNameWithoutExtension(assetFileName);

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SelectAndPing(asset);
            return asset;
        }

        public static T FindSettingsAsset<T>() where T : ScriptableObject {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids == null || guids.Length == 0) return null;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        public static void SelectAndPing(Object asset) {
            if (asset == null) return;
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static void EnsureFolderExists(string folderPath) {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++) {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}

#endif