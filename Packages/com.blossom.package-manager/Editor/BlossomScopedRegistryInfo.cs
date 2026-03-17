using System;
using System.Collections.Generic;

namespace Blossom.PackageManager.Editor {
    [Serializable]
    internal sealed class BlossomScopedRegistryInfo {
        public string Name => name;
        public string Url => url;
        public List<string> Scopes => scopes ??= new();

        public string name;
        public string url;
        public List<string> scopes = new();
    }
}