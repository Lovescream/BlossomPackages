namespace Blossom.Monetization.IAP.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    
    internal sealed class IAPCatalog {

        #region Fields

        private IAPSettingsSO _settings;
        private readonly Dictionary<string, IAPDefinition> _definitionsByKey = new(StringComparer.Ordinal);
        private readonly Dictionary<string, IAPDefinition> _definitionsById = new(StringComparer.Ordinal);
        
        #endregion

        #region Constructor

        internal IAPCatalog(IAPSettingsSO settings) {
            Initialize(settings);
        }

        internal void Initialize(IAPSettingsSO settings) {
            _settings = settings;
            _definitionsByKey.Clear();
            _definitionsById.Clear();

            if (_settings == null) return;
            IAPDefinition[] definitions = _settings.Definitions;
            if (definitions == null || definitions.Length == 0) return;

            foreach (IAPDefinition definition in definitions) {
                if (definition == null) continue;
                if (string.IsNullOrWhiteSpace(definition.Key)) continue;

                if (!_definitionsByKey.TryAdd(definition.Key, definition)) {
                    Debug.LogError($"[Blossom:IAP] Catalog initialization failed. (Key: {definition.Key} is Duplicated.)");
                }

                if (string.IsNullOrWhiteSpace(definition.Id)) continue;

                if (!_definitionsById.TryAdd(definition.Id, definition)) {
                    Debug.LogError($"[Blossom:IAP] Catalog initialization failed. (ID: {definition.Id} is Duplicated.)");
                }
            }
        }

        #endregion

        #region Get

        // Key 기반 상품 정의 조회.
        internal IAPDefinition GetDefinitionByKey(string key) {
            return string.IsNullOrWhiteSpace(key) ? null : _definitionsByKey.GetValueOrDefault(key);
        }

        // ProductID 기반 상품 정의 조회.
        internal IAPDefinition GetDefinitionById(string productId) {
            return string.IsNullOrWhiteSpace(productId) ? null : _definitionsById.GetValueOrDefault(productId);
        }

        internal IReadOnlyCollection<IAPDefinition> GetAllDefinitions() {
            return _definitionsByKey.Values;
        }

        #endregion

        #region Tag

        // 태그 이름 기반 IAPTagId 조회.
        internal bool TryGetTagId(string tagName, out IAPTagId tagId) {
            tagId = default;
            return _settings != null && _settings.TryGetTagId(tagName, out tagId);
        }

        // 태그 이름 조회.
        internal bool TryGetTagName(IAPTagId tagId, out string tagName) {
            tagName = null;
            if (!tagId.IsValid) return false;
            if (_settings == null) return false;

            return _settings.TryGetTagName(tagId, out tagName);
        }

        internal IEnumerable<IAPDefinition> GetDefinitionsByTag(IAPTagId tagId) {
            if (!tagId.IsValid) yield break;

            foreach (IAPDefinition definition in _definitionsByKey.Values) {
                if (definition?.Tags == null || definition.Tags.Count <= 0) continue;
                if (!definition.Tags.Any(tag => tag.Equals(tagId))) continue;
                yield return definition;
            }
        }

        #endregion

    }
}