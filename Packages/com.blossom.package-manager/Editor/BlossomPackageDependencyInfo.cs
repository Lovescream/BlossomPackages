using System;

namespace Blossom.PackageManager.Editor {
    [Serializable]
    internal sealed class BlossomPackageDependencyInfo {
        public string Name => name;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        
        public string name;
        public string displayName;
    }
}