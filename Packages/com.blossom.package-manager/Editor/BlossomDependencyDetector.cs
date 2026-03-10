using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomDependencyDetector {

        public static bool IsInstalled(
            BlossomPackageDependencyInfo dependency,
            HashSet<string> installedPackages) {

            if (dependency == null) return false;

            // 1. 기본적으로 UPM/Git/UnityPackage 류는 package list 먼저 확인
            if (installedPackages != null &&
                !string.IsNullOrWhiteSpace(dependency.Name) &&
                installedPackages.Contains(dependency.Name)) {
                return true;
            }

            // 2. 추가 감지 모드 확인
            switch (dependency.DetectMode) {
                case "TypeExists":
                    return DetectTypeExists(dependency.DetectValue);

                case "DefineSymbol":
                    return DetectDefineSymbol(dependency.DetectValue);

                case "AssetPathAny":
                    return DetectAssetPathAny(dependency.DetectValue);

                case "PackageList":
                default:
                    return installedPackages != null &&
                           !string.IsNullOrWhiteSpace(dependency.Name) &&
                           installedPackages.Contains(dependency.Name);
            }
        }

        private static bool DetectTypeExists(string detectValue) {
            if (string.IsNullOrWhiteSpace(detectValue)) return false;

            string[] candidates = detectValue
                .Split('|')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            // 1차: Type.GetType 직접 시도
            foreach (string candidate in candidates) {
                Type type = Type.GetType(candidate);
                if (type != null) return true;
            }

            // 2차: 로드된 어셈블리 전체 탐색
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (string candidate in candidates) {
                    string fullName = candidate;
                    int commaIndex = candidate.IndexOf(',');
                    if (commaIndex >= 0) {
                        fullName = candidate.Substring(0, commaIndex).Trim();
                    }

                    Type type = assembly.GetType(fullName);
                    if (type != null) return true;
                }
            }

            return false;
        }

        private static bool DetectDefineSymbol(string detectValue) {
            if (string.IsNullOrWhiteSpace(detectValue)) return false;

#pragma warning disable CS0618
            BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#pragma warning restore CS0618

            HashSet<string> defineSet = defines
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet();

            string[] candidates = detectValue
                .Split('|')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            foreach (string candidate in candidates) {
                if (defineSet.Contains(candidate)) {
                    return true;
                }
            }

            return false;
        }

        private static bool DetectAssetPathAny(string detectValue) {
            if (string.IsNullOrWhiteSpace(detectValue)) return false;

            string[] candidates = detectValue
                .Split('|')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();

            string projectRoot = Directory.GetCurrentDirectory();

            foreach (string candidate in candidates) {
                string absolutePath = Path.Combine(projectRoot, candidate);

                if (Directory.Exists(absolutePath) || File.Exists(absolutePath)) {
                    return true;
                }
            }

            return false;
        }
    }
}