using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Blossom.PackageManager.Editor {
    internal sealed class BlossomPackageManagerWindow : EditorWindow {

        private List<BlossomPackageInfo> _packages = new();
        private List<BlossomPackageGroupInfo> _groups = new();

        private HashSet<string> _installed = new();
        private Dictionary<string, string> _installedVersions = new();

        private Vector2 _scroll;
        private bool _isRefreshing;
        private string _statusMessage;

        [MenuItem("Blossom/Package Manager")]
        public static void Open() {
            BlossomPackageManagerWindow window = GetWindow<BlossomPackageManagerWindow>("Blossom Package Manager");
            window.minSize = new(640f, 720f);
            window.Refresh();
        }

        private void OnEnable() {
            Refresh();
        }

        private void Refresh() {
            if (_isRefreshing) return;

            _isRefreshing = true;
            _statusMessage = "Loading catalog...";

            BlossomPackageCatalog.Load((catalogSuccess, catalogMessage) => {
                if (!catalogSuccess) {
                    _isRefreshing = false;
                    _statusMessage = catalogMessage ?? "Catalog load failed.";
                    EditorUtility.DisplayDialog("Catalog Load Failed", _statusMessage, "OK");
                    Repaint();
                    return;
                }

                _packages = new(BlossomPackageCatalog.Packages);
                _groups = new(BlossomPackageCatalog.Groups);

                _statusMessage = "Scanning installed packages...";

                BlossomPackageInstaller.RefreshInstalledPackages((names, versions) => {
                    _installed = new(names);
                    _installedVersions = versions ?? new Dictionary<string, string>();

                    _isRefreshing = false;
                    _statusMessage = catalogMessage;
                    Repaint();
                });
            });
        }

        private void OnGUI() {
            DrawHeader();

            if (_isRefreshing) {
                EditorGUILayout.HelpBox(_statusMessage ?? "Refreshing...", MessageType.Info);
            }
            else if (!string.IsNullOrWhiteSpace(_statusMessage)) {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.None);
            }

            EditorGUILayout.Space(6);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawRequiredSection();
            DrawGroupSections();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader() {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Blossom Package Manager", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Install and manage Blossom packages.");

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !_isRefreshing;
            if (GUILayout.Button("Refresh", GUILayout.Height(26))) {
                Refresh();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }

        private void DrawRequiredSection() {
            List<BlossomPackageInfo> requiredPackages = _packages.Where(p => p.IsRequired).ToList();
            if (requiredPackages.Count == 0) return;

            EditorGUILayout.LabelField("Required", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            foreach (BlossomPackageInfo package in requiredPackages) {
                DrawPackageCard(package);
            }

            EditorGUILayout.Space(4);

            GUI.enabled = !_isRefreshing;
            if (GUILayout.Button("Install Required Packages", GUILayout.Height(28))) {
                InstallRequiredPackages();
            }

            GUI.enabled = true;

            EditorGUILayout.Space(14);
        }

        private void DrawGroupSections() {
            foreach (BlossomPackageGroupInfo group in _groups) {
                if (group.Name == "Required") continue;

                List<BlossomPackageInfo> groupPackages = _packages
                    .Where(p => !p.IsRequired && p.Group == group.Name)
                    .ToList();

                if (groupPackages.Count == 0) continue;

                EditorGUILayout.LabelField(group.DisplayName, EditorStyles.boldLabel);
                EditorGUILayout.Space(4);

                if (group.InstallAll) {
                    GUI.enabled = !_isRefreshing;
                    if (GUILayout.Button($"Install {group.DisplayName} All", GUILayout.Height(26))) {
                        InstallGroup(group.Name);
                    }

                    GUI.enabled = true;

                    EditorGUILayout.Space(6);
                }

                foreach (BlossomPackageInfo package in groupPackages) {
                    DrawPackageCard(package);
                }

                EditorGUILayout.Space(14);
            }
        }

        private void DrawPackageCard(BlossomPackageInfo package) {
            bool isInstalled = _installed.Contains(package.Name);
            string installedVersion = _installedVersions.TryGetValue(package.Name, out string version) ? version : "-";

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(package.DisplayName, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Package: {package.Name}");
            EditorGUILayout.LabelField($"Catalog Version: {package.Version}");
            EditorGUILayout.LabelField($"Installed Version: {installedVersion}");

            if (package.RequiredDependencies.Count > 0) {
                EditorGUILayout.LabelField(
                    $"Required: {string.Join(", ", package.RequiredDependencies.Select(d => d.DisplayName))}");
            }

            if (package.OptionalDependencies.Count > 0) {
                EditorGUILayout.LabelField(
                    $"Optional: {string.Join(", ", package.OptionalDependencies.Select(d => d.DisplayName))}");
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !_isRefreshing;

            if (!isInstalled) {
                if (GUILayout.Button("Install", GUILayout.Height(24))) {
                    TryInstallPackage(package);
                }
            }
            else {
                GUILayout.Label("Installed", GUILayout.Width(70));
                if (GUILayout.Button("Remove", GUILayout.Height(24))) {
                    TryRemovePackage(package);
                }
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void InstallRequiredPackages() {
            foreach (BlossomPackageInfo package in _packages.Where(p => p.IsRequired)) {
                if (_installed.Contains(package.Name)) continue;
                TryInstallPackage(package);
                return;
            }

            EditorUtility.DisplayDialog("Info", "All required packages are already installed.", "OK");
        }

        private void InstallGroup(string groupName) {
            List<BlossomPackageInfo> groupPackages = _packages
                .Where(p => !p.IsRequired && p.Group == groupName)
                .ToList();

            foreach (BlossomPackageInfo package in groupPackages) {
                if (_installed.Contains(package.Name)) continue;
                TryInstallPackage(package);
                return;
            }

            EditorUtility.DisplayDialog("Info", $"All packages in {groupName} are already installed.", "OK");
        }

        private void TryInstallPackage(BlossomPackageInfo package) {
            List<BlossomPackageDependencyInfo> missingRequired = package.RequiredDependencies
                .Where(dep => !_installed.Contains(dep.Name))
                .ToList();

            if (missingRequired.Count > 0) {
                List<BlossomPackageInfo> dependencyPackageInfos = missingRequired
                    .Select(dep => BlossomPackageCatalog.FindPackage(dep.Name))
                    .Where(info => info != null)
                    .ToList();

                if (dependencyPackageInfos.Count > 0) {
                    bool installDependencies = EditorUtility.DisplayDialog(
                        "Required Dependencies",
                        $"{package.DisplayName}를 설치하려면 먼저 다음 패키지가 필요합니다.\n\n- " +
                        $"{string.Join("\n- ", dependencyPackageInfos.Select(p => p.DisplayName))}\n\n" +
                        $"먼저 설치할까요?",
                        "설치",
                        "취소");

                    if (!installDependencies) return;

                    InstallPackageSequence(dependencyPackageInfos, 0, () => { InstallSinglePackage(package); });

                    return;
                }

                EditorUtility.DisplayDialog(
                    "Required Dependencies",
                    $"{package.DisplayName}를 설치하려면 다음 패키지가 먼저 필요합니다.\n\n- " +
                    $"{string.Join("\n- ", missingRequired.Select(d => d.DisplayName))}\n\n" +
                    $"현재 자동 설치할 수 없는 항목이 포함되어 있어 설치를 중단합니다.",
                    "OK");
                return;
            }

            List<BlossomPackageDependencyInfo> missingOptional = package.OptionalDependencies
                .Where(dep => !_installed.Contains(dep.Name))
                .ToList();

            List<BlossomPackageInfo> optionalPackageInfos = missingOptional
                .Select(dep => BlossomPackageCatalog.FindPackage(dep.Name))
                .Where(info => info != null)
                .ToList();

            if (optionalPackageInfos.Count > 0) {
                bool installOptional = EditorUtility.DisplayDialog(
                    "Optional Dependencies",
                    $"{package.DisplayName}는 다음 패키지와 함께 쓰면 좋습니다.\n\n- " +
                    $"{string.Join("\n- ", optionalPackageInfos.Select(p => p.DisplayName))}\n\n" +
                    $"함께 설치할까요?",
                    "함께 설치",
                    "건너뛰기");

                if (installOptional) {
                    InstallPackageSequence(optionalPackageInfos, 0, () => { InstallSinglePackage(package); });
                    return;
                }
            }
            else if (missingOptional.Count > 0) {
                EditorUtility.DisplayDialog(
                    "Optional Dependencies",
                    $"{package.DisplayName}는 다음 외부 패키지와 함께 쓰면 좋습니다.\n\n- " +
                    $"{string.Join("\n- ", missingOptional.Select(d => d.DisplayName))}\n\n" +
                    $"이 패키지들은 자동 설치 대상이 아니므로, 필요하다면 별도로 설치해주세요.",
                    "확인");
            }

            InstallSinglePackage(package);
        }

        private void InstallSinglePackage(BlossomPackageInfo package) {
            string installId = package.BuildInstallId(
                BlossomPackageCatalog.Owner,
                BlossomPackageCatalog.Repo,
                BlossomPackageCatalog.DefaultRef);

            _isRefreshing = true;
            _statusMessage = $"Installing {package.DisplayName}...";

            BlossomPackageInstaller.Install(installId, (success, error) => {
                _isRefreshing = false;

                if (!success) {
                    EditorUtility.DisplayDialog(
                        "Install Failed",
                        error ?? $"Failed to install {package.DisplayName}.",
                        "OK");
                    Refresh();
                    return;
                }

                Refresh();
            });
        }

        private void InstallPackageSequence(List<BlossomPackageInfo> packages, int index, System.Action onComplete) {
            if (packages == null || packages.Count == 0 || index >= packages.Count) {
                onComplete?.Invoke();
                return;
            }

            BlossomPackageInfo package = packages[index];

            if (_installed.Contains(package.Name)) {
                InstallPackageSequence(packages, index + 1, onComplete);
                return;
            }

            string installId = package.BuildInstallId(
                BlossomPackageCatalog.Owner,
                BlossomPackageCatalog.Repo,
                BlossomPackageCatalog.DefaultRef);

            _isRefreshing = true;
            _statusMessage = $"Installing dependency {package.DisplayName}...";

            BlossomPackageInstaller.Install(installId, (success, error) => {
                _isRefreshing = false;

                if (!success) {
                    EditorUtility.DisplayDialog(
                        "Dependency Install Failed",
                        error ?? $"Failed to install dependency: {package.DisplayName}",
                        "OK");
                    Refresh();
                    return;
                }

                RefreshAndContinue(() => { InstallPackageSequence(packages, index + 1, onComplete); });
            });
        }

        private void TryRemovePackage(BlossomPackageInfo package) {
            List<BlossomPackageInfo> dependents = _packages
                .Where(p =>
                    _installed.Contains(p.Name) &&
                    p.RequiredDependencies.Any(dep => dep.Name == package.Name))
                .ToList();

            if (dependents.Count > 0) {
                EditorUtility.DisplayDialog(
                    "Remove Blocked",
                    $"{package.DisplayName}는 현재 다음 패키지에서 사용 중입니다.\n\n- " +
                    $"{string.Join("\n- ", dependents.Select(p => p.DisplayName))}\n\n" +
                    $"먼저 해당 패키지를 제거해주세요.",
                    "확인");
                return;
            }

            bool remove = EditorUtility.DisplayDialog(
                "Remove Package",
                $"{package.DisplayName} 패키지를 제거할까요?",
                "제거",
                "취소");

            if (!remove) return;

            _isRefreshing = true;
            _statusMessage = $"Removing {package.DisplayName}...";

            BlossomPackageInstaller.Remove(package.Name, (success, error) => {
                _isRefreshing = false;

                if (!success) {
                    EditorUtility.DisplayDialog(
                        "Remove Failed",
                        error ?? $"Failed to remove {package.DisplayName}.",
                        "OK");
                    Refresh();
                    return;
                }

                Refresh();
            });
        }

        private void RefreshAndContinue(System.Action onRefreshed) {
            BlossomPackageCatalog.Load((catalogSuccess, catalogMessage) => {
                if (!catalogSuccess) {
                    _isRefreshing = false;
                    _statusMessage = catalogMessage ?? "Catalog load failed.";
                    Repaint();
                    return;
                }

                _packages = new(BlossomPackageCatalog.Packages);
                _groups = new(BlossomPackageCatalog.Groups);

                BlossomPackageInstaller.RefreshInstalledPackages((names, versions) => {
                    _installed = new(names);
                    _installedVersions = versions ?? new Dictionary<string, string>();

                    _isRefreshing = false;
                    _statusMessage = catalogMessage;
                    Repaint();

                    onRefreshed?.Invoke();
                });
            });
        }
    }
}