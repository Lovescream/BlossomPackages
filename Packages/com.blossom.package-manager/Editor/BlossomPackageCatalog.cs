using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomPackageCatalog {

        private const string CatalogUrl =
            "https://raw.githubusercontent.com/Lovescream/BlossomPackages/main/Catalog/catalog.json";

        private const string FallbackCatalogAssetPath =
            "Packages/com.blossom.package-manager/Editor/Resources/catalog.fallback.json";
        
        public static string Owner => _cachedData?.owner ?? string.Empty;
        public static string Repo => _cachedData?.repo ?? string.Empty;
        public static string DefaultRef => _cachedData?.defaultRef ?? "main";
        public static string Version => _cachedData?.DefaultRef ?? string.Empty;
        public static bool IsFallbackCatalog { get; private set; }
        public static string SourceLabel => IsFallbackCatalog ? "catalog.fallback" : "catalog";
        public static IReadOnlyList<BlossomPackageInfo> Packages => _cachedData?.Packages ?? new();
        public static IReadOnlyList<BlossomPackageGroupInfo> Groups => _cachedData?.Groups ?? new();
        
        private static BlossomPackageCatalogData _cachedData;

        public static void Load(Action<bool, string> cbOnComplete) {
            UnityWebRequest request = UnityWebRequest.Get(CatalogUrl);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            EditorApplication.update += WaitRequest;

            void WaitRequest() {
                if (!operation.isDone) return;
                EditorApplication.update -= WaitRequest;

                string remoteText = request.downloadHandler?.text;
                BlossomPackageCatalogData remoteData = null;
                bool remoteSuccess = request.result == UnityWebRequest.Result.Success &&
                                     TryParse(remoteText, out remoteData, out _);
                request.Dispose();

                if (remoteSuccess) {
                    _cachedData = remoteData;
                    IsFallbackCatalog = false;
                    cbOnComplete?.Invoke(true, null);
                    return;
                }

                if (TryLoadFallback(out BlossomPackageCatalogData fallbackData, out string fallbackError)) {
                    _cachedData = fallbackData;
                    IsFallbackCatalog = true;
                    cbOnComplete?.Invoke(true, "Remote catalog load failed. Fallback catalog loaded.");
                    return;
                }

                cbOnComplete?.Invoke(false, fallbackError ?? "Failed to load catalog.");
            }
        }

        public static BlossomPackageInfo FindPackage(string packageName) {
            if (_cachedData == null || string.IsNullOrWhiteSpace(packageName)) return null;
            foreach (BlossomPackageInfo package in _cachedData.Packages) {
                if (package.Name == packageName) return package;
            }

            return null;
        }

        private static bool TryLoadFallback(out BlossomPackageCatalogData data, out string error) {
            data = null;
            error = null;

            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(FallbackCatalogAssetPath);
            if (asset == null) {
                error = $"Fallback catalog not found: {FallbackCatalogAssetPath}";
                return false;
            }

            return TryParse(asset.text, out data, out error);
        }

        private static bool TryParse(string json, out BlossomPackageCatalogData data, out string error) {
            data = null;
            error = null;

            if (string.IsNullOrWhiteSpace(json)) {
                error = "Catalog json is empty.";
                return false;
            }

            try {
                data = JsonUtility.FromJson<BlossomPackageCatalogData>(json);
                if (data == null) {
                    error = "Catalog deserialized to null.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(data.Owner)) {
                    error = "Catalog owner is empty.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(data.Repo)) {
                    error = "Catalog repo is empty.";
                    return false;
                }

                if (data.Packages == null || data.Packages.Count == 0) {
                    error = "Catalog packages are empty.";
                    return false;
                }

                return true;
            }
            catch (Exception ex) {
                error = ex.Message;
                return false;
            }
        }

    }
}