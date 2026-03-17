using System;
using System.Linq;
using UnityEditor;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomScopedRegistryInstaller {

        public static void Install(
            BlossomPackageDependencyInfo dependency,
            Action<bool, string> onComplete) {

            if (dependency == null) {
                onComplete?.Invoke(false, "Dependency is null.");
                return;
            }

            if (string.IsNullOrWhiteSpace(dependency.InstallId)) {
                onComplete?.Invoke(false, "Scoped registry package install id is empty.");
                return;
            }

            if (dependency.Registries == null || dependency.Registries.Count == 0) {
                onComplete?.Invoke(false, "Scoped registry information is missing.");
                return;
            }

            string registrySummary = string.Join(
                "\n",
                dependency.Registries.Select(registry =>
                    $"- {registry.Name} ({registry.Url})\n  Scopes: {string.Join(", ", registry.Scopes)}"));

            bool confirmed = EditorUtility.DisplayDialog(
                "Scoped Registry Installation",
                $"{dependency.DisplayName} 설치를 위해 Packages/manifest.json에 scoped registry를 추가하거나 갱신합니다.\n\n" +
                $"{registrySummary}\n\n" +
                "계속할까요?",
                "계속",
                "취소");

            if (!confirmed) {
                onComplete?.Invoke(false, "Scoped registry installation canceled.");
                return;
            }

            try {
                BlossomUpmManifest manifest = BlossomUpmManifest.Load();

                foreach (BlossomScopedRegistryInfo registry in dependency.Registries) {
                    manifest.AddOrUpdateRegistry(registry.Name, registry.Url, registry.Scopes);
                }

                manifest.Save();
            }
            catch (Exception ex) {
                onComplete?.Invoke(false, ex.Message);
                return;
            }

            BlossomPackageInstaller.Resolve(() => {
                BlossomPackageInstaller.Search(dependency.InstallId, (searchSuccess, version, searchError) => {
                    if (!searchSuccess || string.IsNullOrWhiteSpace(version)) {
                        onComplete?.Invoke(false,
                            searchError ?? $"Failed to find latest version for {dependency.InstallId}.");
                        return;
                    }

                    BlossomPackageInstaller.Install($"{dependency.InstallId}@{version}", (installSuccess, installError) => {
                        if (!installSuccess) {
                            onComplete?.Invoke(false, installError);
                            return;
                        }

                        BlossomPackageInstaller.Resolve(() => { onComplete?.Invoke(true, null); });
                    });
                });
            });
        }
    }
}