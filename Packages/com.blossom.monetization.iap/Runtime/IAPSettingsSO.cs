using System;
using System.Collections.Generic;
using Blossom.Common;
using UnityEngine;

namespace Blossom.Monetization.IAP {
    public class IAPSettingsSO : SettingsSOBase {
        
        public IAPDefinition[] Definitions => definitions;
        public IReadOnlyList<IAPTagDefinition> TagDefinitions => tagDefinitions;
        
        [SerializeField] private IAPDefinition[] definitions;
        [SerializeField] private List<IAPTagDefinition> tagDefinitions = new();

        public bool TryGetTagName(IAPTagId id, out string tagName) {
            foreach (IAPTagDefinition definition in tagDefinitions) {
                if (definition.Guid == id.Guid) {
                    tagName = definition.Name;
                    return true;
                }
            }
            tagName = null;
            return false;
        }

        public bool TryGetTagName(string guid, out string tagName) {
            if (string.IsNullOrEmpty(guid)) {
                tagName = null;
                return false;
            }

            for (int i = 0; i < tagDefinitions.Count; i++) {
                if (tagDefinitions[i].Guid == guid) {
                    tagName = tagDefinitions[i].Name;
                    return true;
                }
            }
            
            tagName = null;
            return false;
        }

        public bool TryGetTagId(string tagName, out IAPTagId tagId, bool ignoreCase = true) {
            tagId = default;
            if (string.IsNullOrWhiteSpace(tagName)) return false;

            StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (IAPTagDefinition definition in tagDefinitions) {
                if (string.Equals(definition.Name, tagName, comparison)) {
                    tagId = new IAPTagId(definition.Guid);
                    return true;
                }
            }

            return false;
        }
        
    }
}