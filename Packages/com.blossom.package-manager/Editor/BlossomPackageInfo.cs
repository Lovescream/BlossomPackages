using System;
using System.Collections.Generic;

namespace Blossom.PackageManager.Editor {
    [Serializable]
    internal sealed class BlossomPackageInfo {
        public string Name => name;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string Version => version;
        public string Group => group;
        public bool IsRequired => isRequired;
        public string Path => path;
        public string RefName => refName;
        public List<BlossomPackageDependencyInfo> RequiredDependencies => requiredDependencies ??= new();
        public List<BlossomPackageDependencyInfo> OptionalDependencies => optionalDependencies ??= new();
        public List<string> InstallDefineSymbols => installDefineSymbols ??= new();
        public List<string> RemoveDefineSymbols => removeDefineSymbols ??= new();

        public string name;
        public string displayName;
        public string version;
        public string group;
        public bool isRequired;
        public string path;
        public string refName;
        public List<BlossomPackageDependencyInfo> requiredDependencies = new();
        public List<BlossomPackageDependencyInfo> optionalDependencies = new();
        public List<string> installDefineSymbols = new();
        public List<string> removeDefineSymbols = new();

        public string BuildInstallId(string owner, string repo, string defaultRef) {
            string gitRef = string.IsNullOrEmpty(RefName) ? defaultRef : RefName;
            return $"https://github.com/{owner}/{repo}.git?path={Path}#{gitRef}";
        }
    }
}