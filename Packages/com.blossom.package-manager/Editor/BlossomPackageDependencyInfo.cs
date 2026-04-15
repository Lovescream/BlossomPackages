using System;
using System.Collections.Generic;

namespace Blossom.PackageManager.Editor {
    [Serializable]
    internal sealed class BlossomPackageDependencyInfo {
        public string Name => name;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string InstallMode => string.IsNullOrWhiteSpace(installMode) ? "CatalogPackage" : installMode;
        public string InstallId => installId;
        public bool AutoInstall => autoInstall;
        public string Note => note;
        public string DetectMode => detectMode;
        public string DetectValue => detectValue;
        public List<BlossomScopedRegistryInfo> Registries => registries ??= new();

        public List<string> InstallDefineSymbols => installDefineSymbols ??= new();
        public List<string> RemoveDefineSymbols => removeDefineSymbols ??= new();

        public string name;
        public string displayName;
        public string installMode;  // CatalogPackage / UnityPackage / GitPackage / ScopedRegistry / Manual
        public string installId;    // UnityPackage: package name, GitPackage: git url, ScopedRegistry: package name
        public bool autoInstall;    // BPM이 자동 설치 시도가 가능한지 여부.
        public string note;         // manual 또는 scoped registry 보완 설명.
        public string detectMode;   // PackageList / TypeExists / DefineSymbol / AssetPathAny / PackageAndDefineSymbol
        public string detectValue;  // 타입명 / define symbol / 경로들
        public List<BlossomScopedRegistryInfo> registries = new();

        public List<string> installDefineSymbols = new();
        public List<string> removeDefineSymbols = new();
    }
}