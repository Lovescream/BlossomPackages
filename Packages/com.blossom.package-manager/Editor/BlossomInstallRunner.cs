using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Blossom.PackageManager.Editor {
    [InitializeOnLoad]
    internal static class BlossomInstallRunner {

        private const string SessionKey_Active = "Blossom.BPM.InstallRunner.Active";
        private const string SessionKey_Mode = "Blossom.BPM.InstallRunner.Mode";
        private const string SessionKey_PackageNames = "Blossom.BPM.InstallRunner.PackageNames";
        private const string SessionKey_DependencyNames = "Blossom.BPM.InstallRunner.DependencyNames";
        private const string SessionKey_DependencyModes = "Blossom.BPM.InstallRunner.DependencyModes";
        private const string SessionKey_CurrentPhase = "Blossom.BPM.InstallRunner.CurrentPhase";
        private const string SessionKey_Index = "Blossom.BPM.InstallRunner.Index";

        private const string Phase_Dependencies = "Dependencies";
        private const string Phase_Packages = "Packages";

        private static bool _isRunning;

        static BlossomInstallRunner() {
            EditorApplication.delayCall += ResumeIfNeeded;
        }

        public static bool IsRunning => _isRunning || SessionState.GetBool(SessionKey_Active, false);

        public static void Start(
            List<BlossomPackageDependencyInfo> dependencies,
            List<BlossomPackageInfo> packages) {

            dependencies ??= new List<BlossomPackageDependencyInfo>();
            packages ??= new List<BlossomPackageInfo>();

            SessionState.SetBool(SessionKey_Active, true);
            SessionState.SetString(SessionKey_CurrentPhase,
                dependencies.Count > 0 ? Phase_Dependencies : Phase_Packages);
            SessionState.SetInt(SessionKey_Index, 0);

            SessionState.SetString(
                SessionKey_DependencyNames,
                string.Join("|", dependencies.Select(x => x.Name)));

            SessionState.SetString(
                SessionKey_DependencyModes,
                string.Join("|", dependencies.Select(x => x.InstallMode)));

            SessionState.SetString(
                SessionKey_PackageNames,
                string.Join("|", packages.Select(x => x.Name)));

            ResumeIfNeeded();
        }

        private static void ResumeIfNeeded() {
            if (_isRunning) return;
            if (!SessionState.GetBool(SessionKey_Active, false)) return;

            _isRunning = true;
            Continue();
        }

        private static void Continue() {
            BlossomPackageCatalog.Load((catalogSuccess, catalogMessage) => {
                if (!catalogSuccess) {
                    StopWithError(catalogMessage ?? "Catalog load failed.");
                    return;
                }

                BlossomPackageInstaller.RefreshInstalledPackages((installedNames, installedVersions) => {
                    HashSet<string> installed = new(installedNames ?? new List<string>());

                    string phase = SessionState.GetString(SessionKey_CurrentPhase, Phase_Packages);
                    int index = SessionState.GetInt(SessionKey_Index, 0);

                    if (phase == Phase_Dependencies) {
                        List<BlossomPackageDependencyInfo> dependencies = RestoreDependencies();
                        while (index < dependencies.Count &&
                               BlossomDependencyDetector.IsInstalled(dependencies[index], installed)) {
                            index++;
                        }

                        if (index >= dependencies.Count) {
                            SessionState.SetString(SessionKey_CurrentPhase, Phase_Packages);
                            SessionState.SetInt(SessionKey_Index, 0);
                            Continue();
                            return;
                        }

                        BlossomPackageDependencyInfo dependency = dependencies[index];
                        BlossomDependencyInstaller.Install(dependency, (success, error) => {
                            if (!success) {
                                StopWithError(error ?? $"Failed to install dependency: {dependency.DisplayName}");
                                return;
                            }

                            SessionState.SetInt(SessionKey_Index, index + 1);
                            EditorApplication.delayCall += Continue;
                        });

                        return;
                    }

                    List<BlossomPackageInfo> packages = RestorePackages();
                    while (index < packages.Count && installed.Contains(packages[index].Name)) {
                        index++;
                    }

                    if (index >= packages.Count) {
                        StopSuccessfully();
                        return;
                    }

                    BlossomPackageInfo package = packages[index];
                    string installId = package.BuildInstallId(
                        BlossomPackageCatalog.Owner,
                        BlossomPackageCatalog.Repo,
                        BlossomPackageCatalog.DefaultRef);

                    BlossomPackageInstaller.Install(installId, (success, error) => {
                        if (!success) {
                            StopWithError(error ?? $"Failed to install package: {package.DisplayName}");
                            return;
                        }

                        ApplyPackagePostInstallActions(package);

                        SessionState.SetInt(SessionKey_Index, index + 1);
                        EditorApplication.delayCall += Continue;
                    });
                });
            });
        }

        private static void ApplyPackagePostInstallActions(BlossomPackageInfo package) {
            if (package == null || package.InstallDefineSymbols == null) return;

            foreach (string symbol in package.InstallDefineSymbols) {
                if (string.IsNullOrWhiteSpace(symbol)) continue;
                BlossomDefineSymbolUtility.AddSymbolToCurrentTarget(symbol);
            }
        }
        
        private static List<BlossomPackageDependencyInfo> RestoreDependencies() {
            string[] names = Split(SessionState.GetString(SessionKey_DependencyNames, ""));
            string[] modes = Split(SessionState.GetString(SessionKey_DependencyModes, ""));

            List<BlossomPackageDependencyInfo> result = new();
            for (int i = 0; i < names.Length; i++) {
                BlossomPackageInfo package = BlossomPackageCatalog.FindPackage(names[i]);
                BlossomPackageDependencyInfo found =
                    package?.RequiredDependencies?.FirstOrDefault(x => x.Name == names[i]) ??
                    BlossomPackageCatalog.Packages
                        .SelectMany(x => x.RequiredDependencies.Concat(x.OptionalDependencies))
                        .FirstOrDefault(x => x.Name == names[i] && x.InstallMode == (i < modes.Length ? modes[i] : x.InstallMode));

                if (found != null) result.Add(found);
            }

            return result;
        }

        private static List<BlossomPackageInfo> RestorePackages() {
            string[] names = Split(SessionState.GetString(SessionKey_PackageNames, ""));
            List<BlossomPackageInfo> result = new();

            foreach (string name in names) {
                BlossomPackageInfo package = BlossomPackageCatalog.FindPackage(name);
                if (package != null) result.Add(package);
            }

            return result;
        }

        private static string[] Split(string value) {
            if (string.IsNullOrWhiteSpace(value)) return Array.Empty<string>();
            return value.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void StopSuccessfully() {
            ClearSession();
            _isRunning = false;

            EditorApplication.delayCall += () => {
                BlossomPackageManagerWindow.Open();
                EditorUtility.DisplayDialog("Install Complete", "모든 패키지 설치가 완료되었습니다.", "OK");
            };
        }

        private static void StopWithError(string message) {
            ClearSession();
            _isRunning = false;

            EditorApplication.delayCall += () => {
                BlossomPackageManagerWindow.Open();
                EditorUtility.DisplayDialog("Install Failed", message, "OK");
            };
        }

        private static void ClearSession() {
            SessionState.EraseBool(SessionKey_Active);
            SessionState.EraseString(SessionKey_Mode);
            SessionState.EraseString(SessionKey_PackageNames);
            SessionState.EraseString(SessionKey_DependencyNames);
            SessionState.EraseString(SessionKey_DependencyModes);
            SessionState.EraseString(SessionKey_CurrentPhase);
            SessionState.EraseInt(SessionKey_Index);
        }
    }
}