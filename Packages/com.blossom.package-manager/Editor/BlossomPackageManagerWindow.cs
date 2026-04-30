using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Blossom.PackageManager.Editor {
    internal sealed class BlossomPackageManagerWindow : EditorWindow {
        
        private const string BpmPackageName = "com.blossom.package-manager";
        private static readonly Color CatalogGreen = new(0.2f, 0.8f, 0.3f);
        private static readonly Color CatalogRed = new(0.85f, 0.25f, 0.25f);

        private List<BlossomPackageInfo> _packages = new();
        private List<BlossomPackageGroupInfo> _groups = new();

        private HashSet<string> _installed = new();
        private Dictionary<string, string> _installedVersions = new();

        private Vector2 _scroll;
        private bool _isRefreshing;
        private string _statusMessage;

        private GUIStyle _titleStyle;
        private GUIStyle _packageNameStyle;
        private GUIStyle _groupHeaderStyle;
        private GUIStyle _groupHeaderButtonStyle;
        private GUIStyle _groupBoxStyle;
        private GUIStyle _cardStyle;
        private GUIStyle _miniLabelStyle;

        private static readonly Color InstalledColor = new(0.2f, 0.7f, 0.25f);
        private static readonly Color MissingColor = new(0.85f, 0.25f, 0.25f);

        private static readonly Color GroupHeaderBg = new(0.18f, 0.18f, 0.18f, 1f);

        private static readonly Color CardBgMissingRequired = new(0.30f, 0.18f, 0.18f, 0.45f);
        private static readonly Color CardBgReadyToInstall = new(0.22f, 0.22f, 0.22f, 0.35f);
        private static readonly Color CardBgInstalledUpdatable = new(0.35f, 0.28f, 0.12f, 0.45f);
        private static readonly Color CardBgInstalledLatest = new(0.16f, 0.28f, 0.18f, 0.45f);

        [MenuItem("Blossom/Package Manager")]
        public static void Open() {
            var window = GetWindow<BlossomPackageManagerWindow>("Blossom Package Manager");
            window.minSize = new Vector2(780f, 720f);
            window.Refresh();
        }

        private void OnEnable() {
            Refresh();
        }

        private void InitializeStyles() {
            if (_titleStyle == null) {
                _titleStyle = new GUIStyle(EditorStyles.boldLabel) {
                    fontSize = 15,
                    richText = true
                };
            }

            if (_packageNameStyle == null) {
                _packageNameStyle = new GUIStyle(EditorStyles.label) {
                    richText = true
                };
            }

            if (_groupHeaderStyle == null) {
                _groupHeaderStyle = new GUIStyle(EditorStyles.boldLabel) {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(10, 10, 6, 6)
                };
            }

            if (_groupHeaderButtonStyle == null) {
                _groupHeaderButtonStyle = new GUIStyle(GUI.skin.button) {
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (_groupBoxStyle == null) {
                _groupBoxStyle = new GUIStyle("box") {
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(0, 0, 8, 14)
                };
            }

            if (_cardStyle == null) {
                _cardStyle = new GUIStyle("box") {
                    padding = new RectOffset(10, 10, 8, 8),
                    margin = new RectOffset(0, 0, 4, 6)
                };
            }

            if (_miniLabelStyle == null) {
                _miniLabelStyle = new GUIStyle(EditorStyles.miniLabel) {
                    richText = true,
                    wordWrap = true
                };
            }
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

                _packages = new List<BlossomPackageInfo>(BlossomPackageCatalog.Packages);
                _groups = new List<BlossomPackageGroupInfo>(BlossomPackageCatalog.Groups);

                _statusMessage = "Scanning installed packages...";

                BlossomPackageInstaller.RefreshInstalledPackages((names, versions) => {
                    _installed = new HashSet<string>(names);
                    _installedVersions = versions ?? new Dictionary<string, string>();

                    _isRefreshing = false;
                    _statusMessage = catalogMessage;
                    Repaint();
                });
            });
        }

        private void OnGUI() {
            InitializeStyles();

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

            using (new EditorGUILayout.HorizontalScope()) {
                using (new EditorGUILayout.VerticalScope()) {
                    EditorGUILayout.LabelField("Blossom Package Manager", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Install and manage Blossom packages.");
                }

                GUILayout.FlexibleSpace();

                DrawTopRightStatus();
            }

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope()) {
                GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;
                if (GUILayout.Button("Refresh", GUILayout.Height(26), GUILayout.Width(100))) {
                    Refresh();
                }

                GUI.enabled = true;
            }

            EditorGUILayout.Space(10);
        }
        
        private void DrawTopRightStatus() {
            string installedBpmVersion = GetInstalledBpmVersion();
            BlossomPackageInfo bpmCatalogPackage = GetCatalogBpmPackage();

            string bpmLabel = string.IsNullOrWhiteSpace(installedBpmVersion)
                ? "BPM: -"
                : $"BPM: {installedBpmVersion}";

            GUILayout.BeginVertical(GUILayout.Width(220f));
            GUILayout.Label(bpmLabel, _miniLabelStyle);

            if (CanUpdateBpm(out BlossomPackageInfo package, out string installedVersion)) {
                GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;
                if (GUILayout.Button($"Update BPM → {package.Version}", GUILayout.Height(20))) {
                    TryUpdateBpm();
                }
                GUI.enabled = true;
            }
            else if (bpmCatalogPackage != null) {
                GUILayout.Label($"Latest: {bpmCatalogPackage.Version}", _miniLabelStyle);
            }

            DrawCatalogStatus();

            GUILayout.EndVertical();
        }
        
        private void DrawCatalogStatus() {
            string catalogVersion = BlossomPackageCatalog.Version;
            string sourceLabel = BlossomPackageCatalog.SourceLabel;
            Color color = BlossomPackageCatalog.IsFallbackCatalog ? CatalogRed : CatalogGreen;

            string colorHex = ToHtmlColor(color);
            string versionText = string.IsNullOrWhiteSpace(catalogVersion) ? "-" : catalogVersion;

            GUILayout.Label(
                $"<color={colorHex}>{sourceLabel}: {versionText}</color>",
                _miniLabelStyle);
        }

        private void DrawRequiredSection() {
            var requiredPackages = _packages.Where(p => p.IsRequired).ToList();
            if (requiredPackages.Count == 0) return;

            DrawGroupHeader("Required Packages", false, null);

            using (new EditorGUILayout.VerticalScope(_groupBoxStyle)) {
                foreach (var package in requiredPackages) {
                    DrawPackageCard(package);
                }

                EditorGUILayout.Space(4);
                
                GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;
                if (GUILayout.Button("Install Required Packages", GUILayout.Height(28))) {
                    InstallRequiredPackages();
                }

                GUI.enabled = true;
            }

            EditorGUILayout.Space(8);
        }

        private void DrawGroupSections() {
            foreach (var group in _groups) {
                if (group.Name == "Required") continue;

                var groupPackages = _packages
                    .Where(p => !p.IsRequired && p.Group == group.Name)
                    .ToList();

                if (groupPackages.Count == 0) continue;

                DrawGroupHeader(
                    group.DisplayName,
                    group.InstallAll,
                    () => InstallGroup(group.Name));

                using (new EditorGUILayout.VerticalScope(_groupBoxStyle)) {
                    foreach (var package in groupPackages) {
                        DrawPackageCard(package);
                    }
                }

                EditorGUILayout.Space(8);
            }
        }

        private void DrawGroupHeader(string title, bool showInstallAll, Action onInstallAll) {
            Rect rect = EditorGUILayout.GetControlRect(false, 30f);
            EditorGUI.DrawRect(rect, GroupHeaderBg);

            Rect labelRect = new Rect(rect.x + 8f, rect.y, rect.width - 16f, rect.height);
            GUI.Label(labelRect, title, _groupHeaderStyle);

            if (!showInstallAll || onInstallAll == null) return;

            float buttonWidth = 130f;
            Rect buttonRect = new Rect(rect.xMax - buttonWidth - 8f, rect.y + 3f, buttonWidth, rect.height - 6f);
            
            GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;
            if (GUI.Button(buttonRect, $"Install {title} All", _groupHeaderButtonStyle)) {
                onInstallAll.Invoke();
            }

            GUI.enabled = true;
        }

        private void DrawPackageCard(BlossomPackageInfo package) {
            bool isInstalled = _installed.Contains(package.Name);
            string installedVersion = _installedVersions.TryGetValue(package.Name, out var version)
                ? version
                : string.Empty;

            BlossomPackageVisualState state = GetVisualState(package, isInstalled, installedVersion);

            Rect bgRect = EditorGUILayout.BeginVertical(_cardStyle);
            DrawCardBackground(bgRect, state);

            DrawTopLine(package, state);
            DrawPackageNameLine(package, isInstalled);
            DrawDependencyLine("Required", package.RequiredDependencies);
            DrawDependencyLine("Optional", package.OptionalDependencies);
            DrawOptionalDependencyActions(package);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawCardBackground(Rect rect, BlossomPackageVisualState state) {
            EditorGUI.DrawRect(rect, GetCardBackgroundColor(state));
        }

        private Color GetCardBackgroundColor(BlossomPackageVisualState state) {
            return state switch {
                BlossomPackageVisualState.NotInstalledMissingRequired => CardBgMissingRequired,
                BlossomPackageVisualState.NotInstalledReady => CardBgReadyToInstall,
                BlossomPackageVisualState.InstalledUpdatable => CardBgInstalledUpdatable,
                BlossomPackageVisualState.InstalledLatest => CardBgInstalledLatest,
                _ => CardBgReadyToInstall
            };
        }

        private void DrawTopLine(BlossomPackageInfo package, BlossomPackageVisualState state) {
            using (new EditorGUILayout.HorizontalScope()) {
                GUILayout.Label($"{package.DisplayName} ({package.Version})", _titleStyle);

                GUILayout.FlexibleSpace();

                GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;

                switch (state) {
                    case BlossomPackageVisualState.NotInstalledMissingRequired:
                        if (GUILayout.Button("Install Required", GUILayout.Width(130), GUILayout.Height(24))) {
                            InstallMissingRequiredFor(package, true);
                        }

                        break;

                    case BlossomPackageVisualState.NotInstalledReady:
                        if (GUILayout.Button("Install", GUILayout.Width(100), GUILayout.Height(24))) {
                            TryInstallPackage(package);
                        }

                        break;

                    case BlossomPackageVisualState.InstalledUpdatable:
                        if (GUILayout.Button("Update", GUILayout.Width(100), GUILayout.Height(24))) {
                            TryUpdatePackage(package);
                        }

                        break;

                    case BlossomPackageVisualState.InstalledLatest:
                        if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.Height(24))) {
                            TryRemovePackage(package);
                        }

                        break;
                }

                GUI.enabled = true;
            }

            if (state == BlossomPackageVisualState.InstalledUpdatable) {
                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.FlexibleSpace();

                    GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;
                    if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.Height(22))) {
                        TryRemovePackage(package);
                    }

                    GUI.enabled = true;
                }
            }
        }

        private void DrawPackageNameLine(BlossomPackageInfo package, bool isInstalled) {
            string colorHex = isInstalled ? ToHtmlColor(InstalledColor) : "#FFFFFF";
            EditorGUILayout.LabelField($"<color={colorHex}>{package.Name}</color>", _packageNameStyle);
        }

        private void DrawDependencyLine(string label, List<BlossomPackageDependencyInfo> dependencies) {
            if (dependencies == null || dependencies.Count == 0) return;

            string joined = string.Join(", ", dependencies.Select(FormatDependencyText));
            EditorGUILayout.LabelField($"{label}: {joined}", _miniLabelStyle);
        }
        
        private void DrawOptionalDependencyActions(BlossomPackageInfo package) {
            if (package == null || package.OptionalDependencies == null || package.OptionalDependencies.Count == 0) return;

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Optional Actions", _miniLabelStyle);

            foreach (BlossomPackageDependencyInfo dependency in package.OptionalDependencies) {
                bool installed = IsDependencyInstalled(dependency);

                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.Label($"- {dependency.DisplayName}", _miniLabelStyle);
                    GUILayout.FlexibleSpace();

                    GUI.enabled = !_isRefreshing && !BlossomInstallRunner.IsRunning;

                    if (!installed) {
                        if (GUILayout.Button($"Install {dependency.DisplayName}", GUILayout.Width(160), GUILayout.Height(22))) {
                            InstallOptionalDependency(dependency);
                        }
                    }
                    else {
                        if (GUILayout.Button($"Remove {dependency.DisplayName}", GUILayout.Width(160), GUILayout.Height(22))) {
                            RemoveOptionalDependency(dependency);
                        }
                    }

                    GUI.enabled = true;
                }
            }
        }
        
        private void InstallOptionalDependency(BlossomPackageDependencyInfo dependency) {
            if (dependency == null) return;

            if (!IsDependencyAutoInstallable(dependency)) {
                EditorUtility.DisplayDialog(
                    "Optional Dependency",
                    GetDependencyInstallMessage(dependency),
                    "OK");
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "Install Optional Dependency",
                $"{dependency.DisplayName} 를 설치할까요?",
                "설치",
                "취소");

            if (!ok) return;

            BlossomInstallRunner.Start(
                new List<BlossomPackageDependencyInfo> { dependency },
                new List<BlossomPackageInfo>());

            Refresh();
        }
        
        private void RemoveOptionalDependency(BlossomPackageDependencyInfo dependency) {
            if (dependency == null) return;

            string packageName = string.IsNullOrWhiteSpace(dependency.InstallId)
                ? dependency.Name
                : dependency.InstallId;

            bool ok = EditorUtility.DisplayDialog(
                "Remove Optional Dependency",
                $"{dependency.DisplayName} 를 제거할까요?",
                "제거",
                "취소");

            if (!ok) return;

            _isRefreshing = true;
            _statusMessage = $"Removing {dependency.DisplayName}...";

            BlossomPackageInstaller.Remove(packageName, (success, error) => {
                if (!success) {
                    _isRefreshing = false;
                    EditorUtility.DisplayDialog(
                        "Remove Failed",
                        error ?? $"Failed to remove {dependency.DisplayName}.",
                        "OK");
                    Refresh();
                    return;
                }

                BlossomDependencyInstaller.ApplyPostRemoveActions(dependency);

                BlossomPackageInstaller.Resolve(() => {
                    _isRefreshing = false;
                    Refresh();

                    EditorApplication.delayCall += () => {
                        EditorUtility.DisplayDialog(
                            "Remove Complete",
                            $"{dependency.DisplayName} 제거가 완료되었습니다.",
                            "OK");
                    };
                });
            });
        }

        private string FormatDependencyText(BlossomPackageDependencyInfo dependency) {
            bool installed = IsDependencyInstalled(dependency);
            string colorHex = installed ? ToHtmlColor(InstalledColor) : ToHtmlColor(MissingColor);
            return $"<color={colorHex}>{dependency.DisplayName}</color>";
        }

        private BlossomPackageVisualState GetVisualState(
            BlossomPackageInfo package,
            bool isInstalled,
            string installedVersion) {

            if (!isInstalled) {
                bool missingRequired = package.RequiredDependencies.Any(dep => !IsDependencyInstalled(dep));
                return missingRequired
                    ? BlossomPackageVisualState.NotInstalledMissingRequired
                    : BlossomPackageVisualState.NotInstalledReady;
            }

            bool hasUpdate = IsVersionLower(installedVersion, package.Version);
            return hasUpdate
                ? BlossomPackageVisualState.InstalledUpdatable
                : BlossomPackageVisualState.InstalledLatest;
        }

        private bool IsVersionLower(string installedVersion, string catalogVersion) {
            if (string.IsNullOrWhiteSpace(installedVersion)) return true;
            if (string.IsNullOrWhiteSpace(catalogVersion)) return false;

            Version installedParsed = ParseVersion(installedVersion);
            Version catalogParsed = ParseVersion(catalogVersion);
            return installedParsed < catalogParsed;
        }

        private Version ParseVersion(string version) {
            if (string.IsNullOrWhiteSpace(version)) return new Version(0, 0, 0);

            string[] parts = version.Split('.');
            int major = parts.Length > 0 && int.TryParse(parts[0], out int a) ? a : 0;
            int minor = parts.Length > 1 && int.TryParse(parts[1], out int b) ? b : 0;
            int build = parts.Length > 2 && int.TryParse(parts[2], out int c) ? c : 0;

            return new Version(major, minor, build);
        }

        private string ToHtmlColor(Color color) {
            return $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }

        private bool IsDependencyInstalled(BlossomPackageDependencyInfo dependency) {
            return BlossomDependencyDetector.IsInstalled(dependency, _installed);
        }

        private bool HasMissingOptionalDependencies(BlossomPackageInfo package) {
            return package.OptionalDependencies.Any(dep => !IsDependencyInstalled(dep));
        }

        private void InstallRequiredPackages() {
            var missingRequired = _packages
                .Where(p => p.IsRequired && !_installed.Contains(p.Name))
                .ToList();

            if (missingRequired.Count == 0) {
                EditorUtility.DisplayDialog("Info", "All required packages are already installed.", "OK");
                return;
            }
            
            BlossomInstallRunner.Start(new List<BlossomPackageDependencyInfo>(), missingRequired);
            Refresh();
        }

        private void InstallGroup(string groupName) {
            var targets = _packages
                .Where(p => !p.IsRequired && p.Group == groupName && !_installed.Contains(p.Name))
                .ToList();

            if (targets.Count == 0) {
                EditorUtility.DisplayDialog("Info", $"All packages in {groupName} are already installed.", "OK");
                return;
            }

            InstallPlan plan = BuildInstallPlan(targets);

            if (plan.ManualDependencies.Count > 0) {
                EditorUtility.DisplayDialog(
                    "Required Dependencies",
                    $"다음 필수 의존성은 자동 설치할 수 없습니다.\n\n" +
                    string.Join("\n", plan.ManualDependencies.Select(
                        d => $"- {d.DisplayName}: {GetDependencyInstallMessage(d)}")),
                    "OK");
                return;
            }

            string message =
                $"[{groupName}] 그룹 설치를 진행합니다.\n\n" +
                (plan.DirectDependencies.Count > 0
                    ? "[외부 의존성]\n" + string.Join("\n", plan.DirectDependencies.Select(d => $"- {d.DisplayName}")) + "\n\n"
                    : "") +
                (plan.CatalogPackages.Count > 0
                    ? "[Blossom 패키지]\n" + string.Join("\n", plan.CatalogPackages.Select(p => $"- {p.DisplayName}")) + "\n\n"
                    : "");

            bool ok = EditorUtility.DisplayDialog(
                "Install Group",
                message,
                "설치",
                "취소");

            if (!ok) return;

            BlossomInstallRunner.Start(plan.DirectDependencies, plan.CatalogPackages);
            Refresh();
        }
        private void InstallMissingRequiredFor(BlossomPackageInfo package, bool installTargetAfterDependencies) {
            var missingRequired = package.RequiredDependencies
                .Where(dep => !IsDependencyInstalled(dep))
                .ToList();

            if (missingRequired.Count == 0) {
                if (installTargetAfterDependencies) {
                    TryInstallPackage(package);
                }
                else {
                    Refresh();
                }

                return;
            }

            var autoInstallable = missingRequired
                .Where(IsDependencyAutoInstallable)
                .ToList();

            var manual = missingRequired
                .Where(dep => !IsDependencyAutoInstallable(dep))
                .ToList();

            if (manual.Count > 0) {
                EditorUtility.DisplayDialog(
                    "Required Dependencies",
                    $"{package.DisplayName}에는 자동 설치할 수 없는 필수 의존성이 있습니다.\n\n" +
                    string.Join("\n", manual.Select(d => $"- {d.DisplayName}: {GetDependencyInstallMessage(d)}")),
                    "OK");
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "Install Required Dependencies",
                $"{package.DisplayName}를 위해 다음 필수 의존성을 설치할까요?\n\n- " +
                $"{string.Join("\n- ", autoInstallable.Select(d => d.DisplayName))}",
                "설치",
                "취소");

            if (!ok) return;

            List<BlossomPackageInfo> targetPackages = installTargetAfterDependencies
                ? new List<BlossomPackageInfo> { package }
                : new List<BlossomPackageInfo>();

            BlossomInstallRunner.Start(autoInstallable, targetPackages);
            Refresh();
        }

        private InstallPlan BuildInstallPlan(IEnumerable<BlossomPackageInfo> targets) {
            InstallPlan plan = new();

            HashSet<string> visitedPackages = new();
            HashSet<string> addedCatalogPackages = new();
            HashSet<string> addedDirectDependencies = new();
            HashSet<string> addedManualDependencies = new();

            foreach (BlossomPackageInfo target in targets) {
                ResolvePackageRecursive(
                    target,
                    plan,
                    visitedPackages,
                    addedCatalogPackages,
                    addedDirectDependencies,
                    addedManualDependencies);
            }

            return plan;
        }

        private void ResolvePackageRecursive(
            BlossomPackageInfo package,
            InstallPlan plan,
            HashSet<string> visitedPackages,
            HashSet<string> addedCatalogPackages,
            HashSet<string> addedDirectDependencies,
            HashSet<string> addedManualDependencies) {

            if (package == null) return;
            if (_installed.Contains(package.Name)) return;
            if (!visitedPackages.Add(package.Name)) return;

            foreach (BlossomPackageDependencyInfo dependency in package.RequiredDependencies) {
                if (IsDependencyInstalled(dependency)) continue;

                switch (dependency.InstallMode) {
                    case "CatalogPackage": {
                            BlossomPackageInfo dependencyPackage = BlossomPackageCatalog.FindPackage(dependency.Name);

                            if (dependencyPackage == null) {
                                if (addedManualDependencies.Add(dependency.Name)) {
                                    plan.ManualDependencies.Add(new BlossomPackageDependencyInfo {
                                        name = dependency.Name,
                                        displayName = dependency.DisplayName,
                                        installMode = dependency.InstallMode,
                                        autoInstall = false,
                                        note = $"Catalog package not found: {dependency.Name}"
                                    });
                                }

                                break;
                            }

                            ResolvePackageRecursive(
                                dependencyPackage,
                                plan,
                                visitedPackages,
                                addedCatalogPackages,
                                addedDirectDependencies,
                                addedManualDependencies);

                            break;
                        }

                    case "UnityPackage":
                    case "GitPackage": {
                            if (dependency.AutoInstall) {
                                if (addedDirectDependencies.Add(dependency.Name)) {
                                    plan.DirectDependencies.Add(dependency);
                                }
                            }
                            else {
                                if (addedManualDependencies.Add(dependency.Name)) {
                                    plan.ManualDependencies.Add(dependency);
                                }
                            }

                            break;
                        }

                    case "ScopedRegistry": {
                            if (dependency.AutoInstall) {
                                if (addedDirectDependencies.Add(dependency.Name)) {
                                    plan.DirectDependencies.Add(dependency);
                                }
                            }
                            else {
                                if (addedManualDependencies.Add(dependency.Name)) {
                                    plan.ManualDependencies.Add(dependency);
                                }
                            }

                            break;
                        }

                    case "Manual":
                    default: {
                            if (addedManualDependencies.Add(dependency.Name)) {
                                plan.ManualDependencies.Add(dependency);
                            }

                            break;
                        }
                }
            }

            if (addedCatalogPackages.Add(package.Name)) {
                plan.CatalogPackages.Add(package);
            }
        }
        private void InstallMissingOptionalFor(BlossomPackageInfo package) {
            var missingOptional = package.OptionalDependencies
                .Where(dep => !IsDependencyInstalled(dep))
                .ToList();

            if (missingOptional.Count == 0) {
                EditorUtility.DisplayDialog("Info", "All optional dependencies are already installed.", "OK");
                Refresh();
                return;
            }

            var autoInstallable = missingOptional
                .Where(IsDependencyAutoInstallable)
                .ToList();

            var manual = missingOptional
                .Where(dep => !IsDependencyAutoInstallable(dep))
                .ToList();

            if (autoInstallable.Count > 0) {
                bool ok = EditorUtility.DisplayDialog(
                    "Install Optional Dependencies",
                    $"{package.DisplayName}의 선택 의존성을 설치할까요?\n\n- " +
                    $"{string.Join("\n- ", autoInstallable.Select(d => d.DisplayName))}",
                    "설치",
                    "취소");

                if (ok) {
                    BlossomInstallRunner.Start(autoInstallable, new List<BlossomPackageInfo>());
                    ShowManualDependencyDialogIfNeeded("Optional Dependencies", manual);
                    Refresh();
                    return;
                }
            }

            ShowManualDependencyDialogIfNeeded("Optional Dependencies", manual);
            Refresh();
        }

        private void TryInstallPackage(BlossomPackageInfo package) {
            var missingRequired = package.RequiredDependencies
                .Where(dep => !IsDependencyInstalled(dep))
                .ToList();

            if (missingRequired.Count > 0) {
                InstallMissingRequiredFor(package, true);
                return;
            }

            var missingOptional = package.OptionalDependencies
                .Where(dep => !IsDependencyInstalled(dep))
                .ToList();

            if (missingOptional.Count > 0) {
                bool installOptional = EditorUtility.DisplayDialog(
                    "Optional Dependencies",
                    $"{package.DisplayName}는 선택 의존성이 있습니다.\n\n- " +
                    $"{string.Join("\n- ", missingOptional.Select(d => d.DisplayName))}\n\n" +
                    "선택 의존성을 함께 설치할까요?",
                    "함께 설치",
                    "건너뛰기");

                if (installOptional) {
                    InstallMissingOptionalFor(package);
                    return;
                }
            }

            InstallSinglePackage(package);
        }

        private void TryUpdatePackage(BlossomPackageInfo package) {
            bool ok = EditorUtility.DisplayDialog(
                "Update Package",
                $"{package.DisplayName} 패키지를 catalog 버전 {package.Version}으로 업데이트할까요?",
                "업데이트",
                "취소");

            if (!ok) return;

            InstallSinglePackage(package);
        }

        private void InstallSinglePackage(BlossomPackageInfo package) {
            if (package == null) return;

            BlossomInstallRunner.Start(
                new List<BlossomPackageDependencyInfo>(),
                new List<BlossomPackageInfo> { package });

            Refresh();
        }
        
        private bool IsDependencyAutoInstallable(BlossomPackageDependencyInfo dependency) {
            if (dependency == null || !dependency.AutoInstall) return false;

            return dependency.InstallMode == "CatalogPackage" ||
                   dependency.InstallMode == "UnityPackage" ||
                   dependency.InstallMode == "GitPackage" ||
                   dependency.InstallMode == "ScopedRegistry";
        }

        private string GetDependencyInstallMessage(BlossomPackageDependencyInfo dependency) {
            return string.IsNullOrWhiteSpace(dependency.Note)
                ? $"Install mode: {dependency.InstallMode}"
                : dependency.Note;
        }

        private void ShowManualDependencyDialogIfNeeded(
            string title,
            List<BlossomPackageDependencyInfo> manualDependencies) {

            if (manualDependencies == null || manualDependencies.Count == 0) return;

            EditorUtility.DisplayDialog(
                title,
                string.Join("\n", manualDependencies.Select(
                    d => $"- {d.DisplayName}: {GetDependencyInstallMessage(d)}")),
                "OK");
        }
        
        private void TryRemovePackage(BlossomPackageInfo package) {
            var dependents = _packages
                .Where(p =>
                    _installed.Contains(p.Name) &&
                    p.RequiredDependencies.Any(dep => dep.Name == package.Name))
                .ToList();

            if (dependents.Count > 0) {
                EditorUtility.DisplayDialog(
                    "Remove Blocked",
                    $"{package.DisplayName}는 현재 다음 패키지에서 사용 중입니다.\n\n- " +
                    $"{string.Join("\n- ", dependents.Select(p => p.DisplayName))}\n\n" +
                    "먼저 해당 패키지를 제거해주세요.",
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
                if (!success) {
                    _isRefreshing = false;
                    EditorUtility.DisplayDialog(
                        "Remove Failed",
                        error ?? $"Failed to remove {package.DisplayName}.",
                        "OK");
                    Refresh();
                    return;
                }

                ApplyPackagePostRemoveActions(package);

                BlossomPackageInstaller.Resolve(() => {
                    _isRefreshing = false;
                    Refresh();

                    EditorApplication.delayCall += () => {
                        EditorUtility.DisplayDialog(
                            "Remove Complete",
                            $"{package.DisplayName} 패키지 제거가 완료되었습니다.",
                            "OK");
                    };
                });
            });
        }
        
        private void ApplyPackagePostRemoveActions(BlossomPackageInfo package) {
            if (package == null || package.RemoveDefineSymbols == null) return;

            foreach (string symbol in package.RemoveDefineSymbols) {
                if (string.IsNullOrWhiteSpace(symbol)) continue;
                BlossomDefineSymbolUtility.RemoveSymbolFromSupportedTargets(symbol);
            }
        }
        
        private string GetInstalledBpmVersion() {
            return _installedVersions.TryGetValue(BpmPackageName, out string version)
                ? version
                : string.Empty;
        }

        private BlossomPackageInfo GetCatalogBpmPackage() {
            return _packages.FirstOrDefault(x => x.Name == BpmPackageName);
        }

        private bool CanUpdateBpm(out BlossomPackageInfo bpmPackage, out string installedVersion) {
            bpmPackage = GetCatalogBpmPackage();
            installedVersion = GetInstalledBpmVersion();

            if (bpmPackage == null) return false;
            if (!_installed.Contains(BpmPackageName)) return false;

            return IsVersionLower(installedVersion, bpmPackage.Version);
        }

        private void TryUpdateBpm() {
            BlossomPackageInfo bpmPackage = GetCatalogBpmPackage();
            if (bpmPackage == null) {
                EditorUtility.DisplayDialog("BPM Update", "Catalog package not found: com.blossom.package-manager", "OK");
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "Update BPM",
                $"BPM을 {bpmPackage.Version} 버전으로 업데이트할까요?",
                "업데이트",
                "취소");

            if (!ok) return;

            InstallSinglePackage(bpmPackage);
        }

        private sealed class InstallPlan {
            public readonly List<BlossomPackageInfo> CatalogPackages = new();
            public readonly List<BlossomPackageDependencyInfo> DirectDependencies = new();
            public readonly List<BlossomPackageDependencyInfo> ManualDependencies = new();
        }
    }
}