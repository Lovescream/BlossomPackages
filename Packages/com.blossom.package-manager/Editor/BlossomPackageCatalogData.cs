using System;
using System.Collections.Generic;

namespace Blossom.PackageManager.Editor {
    [Serializable]
    internal sealed class BlossomPackageCatalogData {
        public string Owner => owner;
        public string Repo => repo;
        public string DefaultRef => string.IsNullOrWhiteSpace(defaultRef) ? "main" : defaultRef;
        public List<BlossomPackageGroupInfo> Groups => groups ??= new();
        public List<BlossomPackageInfo> Packages => packages ??= new();
        
        public string owner;
        public string repo;
        public string defaultRef;
        public List<BlossomPackageGroupInfo> groups = new();
        public List<BlossomPackageInfo> packages = new();
    }
}