using System;

namespace Blossom.PackageManager.Editor {
    [Serializable]
    internal sealed class BlossomPackageGroupInfo {
        public string Name => name;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public bool InstallAll => installAll;
        
        public string name;
        public string displayName;
        public bool installAll;
    }
}