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
                    InstallScopedRegistryPackage(dependency, onComplete);
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

            BlossomPackageInfo package = BlossomPackageCatalog.FindPackage(dependency.Name);
            if (package == null) {
                onComplete?.Invoke(false, $"Catalog package not found: {dependency.Name}");
                return;
            }

            string installId = package.BuildInstallId(
                BlossomPackageCatalog.Owner,
                BlossomPackageCatalog.Repo,
                BlossomPackageCatalog.DefaultRef);

            BlossomPackageInstaller.Install(installId, (success, error) => {
                if (success) ApplyPostInstallActions(dependency);
                onComplete?.Invoke(success, error);
            });
        }

        private static void InstallDirectPackage(
            BlossomPackageDependencyInfo dependency,
            Action<bool, string> onComplete) {

            if (string.IsNullOrWhiteSpace(dependency.InstallId)) {
                onComplete?.Invoke(false, $"InstallId is empty: {dependency.DisplayName}");
                return;
            }

            BlossomPackageInstaller.Install(dependency.InstallId, (success, error) => {
                if (success) ApplyPostInstallActions(dependency);
                onComplete?.Invoke(success, error);
            });
        }

        private static void InstallScopedRegistryPackage(
            BlossomPackageDependencyInfo dependency,
            Action<bool, string> onComplete) {

            BlossomScopedRegistryInstaller.Install(dependency, (success, error) => {
                if (success) ApplyPostInstallActions(dependency);
                onComplete?.Invoke(success, error);
            });
        }

        internal static void ApplyPostInstallActions(BlossomPackageDependencyInfo dependency) {
            if (dependency == null || dependency.InstallDefineSymbols == null) return;

            foreach (string symbol in dependency.InstallDefineSymbols) {
                if (string.IsNullOrWhiteSpace(symbol)) continue;
                BlossomDefineSymbolUtility.AddSymbolToCurrentTarget(symbol);
            }
        }

        internal static void ApplyPostRemoveActions(BlossomPackageDependencyInfo dependency) {
            if (dependency == null || dependency.RemoveDefineSymbols == null) return;

            foreach (string symbol in dependency.RemoveDefineSymbols) {
                if (string.IsNullOrWhiteSpace(symbol)) continue;
                BlossomDefineSymbolUtility.RemoveSymbolFromCurrentTarget(symbol);
            }
        }
    }
}