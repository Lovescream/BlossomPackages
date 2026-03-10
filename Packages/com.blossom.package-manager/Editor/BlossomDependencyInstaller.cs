using System;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomDependencyInstaller {

        public static void Install(
            BlossomPackageDependencyInfo dependency,
            Action<bool, string> onComplete) {

            if (dependency == null) {
                onComplete?.Invoke(false, "Dependency is null.");
                return;
            }

            switch (dependency.InstallMode) {
                case "CatalogPackage":
                    InstallCatalogPackage(dependency, onComplete);
                    return;

                case "UnityPackage":
                case "GitPackage":
                    InstallDirectPackage(dependency, onComplete);
                    return;

                case "ScopedRegistry":
                    onComplete?.Invoke(false,
                        string.IsNullOrWhiteSpace(dependency.Note)
                            ? "Scoped registry setup is required."
                            : dependency.Note);
                    return;

                case "Manual":
                    onComplete?.Invoke(false,
                        string.IsNullOrWhiteSpace(dependency.Note)
                            ? "This dependency must be installed manually."
                            : dependency.Note);
                    return;

                default:
                    onComplete?.Invoke(false, $"Unknown install mode: {dependency.InstallMode}");
                    return;
            }
        }

        private static void InstallCatalogPackage(
            BlossomPackageDependencyInfo dependency,
            Action<bool, string> onComplete) {

            var package = BlossomPackageCatalog.FindPackage(dependency.Name);
            if (package == null) {
                onComplete?.Invoke(false, $"Catalog package not found: {dependency.Name}");
                return;
            }

            string installId = package.BuildInstallId(
                BlossomPackageCatalog.Owner,
                BlossomPackageCatalog.Repo,
                BlossomPackageCatalog.DefaultRef);

            BlossomPackageInstaller.Install(installId, onComplete);
        }

        private static void InstallDirectPackage(
            BlossomPackageDependencyInfo dependency,
            Action<bool, string> onComplete) {

            if (string.IsNullOrWhiteSpace(dependency.InstallId)) {
                onComplete?.Invoke(false, $"InstallId is empty: {dependency.DisplayName}");
                return;
            }

            BlossomPackageInstaller.Install(dependency.InstallId, onComplete);
        }
    }
}