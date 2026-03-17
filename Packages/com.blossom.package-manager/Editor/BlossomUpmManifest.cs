using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blossom.PackageManager.Editor {
    internal sealed class BlossomUpmManifest {

        private const string KeyName = "name";
        private const string KeyUrl = "url";
        private const string KeyScopes = "scopes";
        private const string KeyScopedRegistries = "scopedRegistries";
        private const string KeyDependencies = "dependencies";

        private readonly Dictionary<string, object> _manifest;

        private static string ManifestPath =>
            Path.Combine(Directory.GetCurrentDirectory(), "Packages/manifest.json");

        private BlossomUpmManifest(Dictionary<string, object> manifest) {
            _manifest = manifest;
        }

        public static BlossomUpmManifest Load() {
            if (!File.Exists(ManifestPath)) {
                throw new Exception($"Manifest not found: {ManifestPath}");
            }

            string json = File.ReadAllText(ManifestPath);
            if (string.IsNullOrWhiteSpace(json)) {
                throw new Exception("manifest.json is empty.");
            }

            if (BlossomMiniJson.Deserialize(json) is not Dictionary<string, object> manifest) {
                throw new Exception("Failed to parse manifest.json");
            }

            return new BlossomUpmManifest(manifest);
        }

        public void AddOrUpdateRegistry(string name, string url, List<string> scopes) {
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("Registry name is empty.");
            if (string.IsNullOrWhiteSpace(url)) throw new Exception("Registry url is empty.");
            if (scopes == null || scopes.Count == 0) throw new Exception("Registry scopes are empty.");

            List<object> registries = GetOrCreateScopedRegistries();

            Dictionary<string, object> existing = registries
                .OfType<Dictionary<string, object>>()
                .FirstOrDefault(x => GetString(x, KeyName) == name);

            if (existing == null) {
                registries.Add(new Dictionary<string, object> {
                    { KeyName, name },
                    { KeyUrl, url },
                    { KeyScopes, scopes.Cast<object>().ToList() }
                });
                return;
            }

            existing[KeyUrl] = url;

            List<object> existingScopes = GetList(existing, KeyScopes) ?? new List<object>();
            HashSet<string> merged = existingScopes
                .Select(x => x?.ToString() ?? string.Empty)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet();

            foreach (string scope in scopes) {
                if (!string.IsNullOrWhiteSpace(scope)) {
                    merged.Add(scope);
                }
            }

            existing[KeyScopes] = merged.Cast<object>().ToList();
        }

        public void AddPackageDependency(string packageName, string versionOrSource) {
            Dictionary<string, object> dependencies = GetDependencies();
            dependencies[packageName] = versionOrSource;
        }

        public void Save() {
            string json = BlossomMiniJson.Serialize(_manifest, true);
            File.WriteAllText(ManifestPath, json);
        }

        private Dictionary<string, object> GetDependencies() {
            if (!_manifest.TryGetValue(KeyDependencies, out object value) ||
                value is not Dictionary<string, object> dict) {
                throw new Exception("No dependencies section found in manifest.json");
            }

            return dict;
        }

        private List<object> GetOrCreateScopedRegistries() {
            if (_manifest.TryGetValue(KeyScopedRegistries, out object value) &&
                value is List<object> list) {
                return list;
            }

            List<object> created = new();
            _manifest[KeyScopedRegistries] = created;
            return created;
        }

        private static string GetString(IDictionary<string, object> dict, string key) {
            if (dict == null) return string.Empty;

            return dict.TryGetValue(key, out object value) && value != null
                ? value.ToString()
                : string.Empty;
        }

        private static List<object> GetList(IDictionary<string, object> dict, string key) {
            if (dict == null) return null;
            return dict.TryGetValue(key, out object value) ? value as List<object> : null;
        }
    }
}