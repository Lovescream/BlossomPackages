using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace Blossom.PackageManager.Editor {
    internal static class BlossomDefineSymbolUtility {

        private static readonly BuildTargetGroup[] SupportedGroups = {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS
        };

        public static void AddSymbolToSupportedTargets(string symbol) {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            foreach (BuildTargetGroup group in SupportedGroups) {
                AddSymbolToGroup(group, symbol);
            }
        }

        public static void RemoveSymbolFromSupportedTargets(string symbol) {
            if (string.IsNullOrWhiteSpace(symbol)) return;

            foreach (BuildTargetGroup group in SupportedGroups) {
                RemoveSymbolFromGroup(group, symbol);
            }
        }

        private static void AddSymbolToGroup(BuildTargetGroup group, string symbol) {
#if UNITY_2021_2_OR_NEWER
            NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(group);
            string defines = PlayerSettings.GetScriptingDefineSymbols(target);
            string updated = AddSymbol(defines, symbol);

            if (defines != updated) {
                PlayerSettings.SetScriptingDefineSymbols(target, updated);
            }
#else
#pragma warning disable CS0618
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string updated = AddSymbol(defines, symbol);

            if (defines != updated) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, updated);
            }
#pragma warning restore CS0618
#endif
        }

        private static void RemoveSymbolFromGroup(BuildTargetGroup group, string symbol) {
#if UNITY_2021_2_OR_NEWER
            NamedBuildTarget target = NamedBuildTarget.FromBuildTargetGroup(group);
            string defines = PlayerSettings.GetScriptingDefineSymbols(target);
            string updated = RemoveSymbol(defines, symbol);

            if (defines != updated) {
                PlayerSettings.SetScriptingDefineSymbols(target, updated);
            }
#else
#pragma warning disable CS0618
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            string updated = RemoveSymbol(defines, symbol);

            if (defines != updated) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, updated);
            }
#pragma warning restore CS0618
#endif
        }

        private static string AddSymbol(string defines, string symbol) {
            string[] items = (defines ?? string.Empty)
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (items.Contains(symbol, StringComparer.Ordinal)) {
                return string.Join(";", items);
            }

            return items.Length == 0
                ? symbol
                : string.Join(";", items) + ";" + symbol;
        }

        private static string RemoveSymbol(string defines, string symbol) {
            string[] items = (defines ?? string.Empty)
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Where(x => !string.Equals(x, symbol, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            return string.Join(";", items);
        }
    }
}