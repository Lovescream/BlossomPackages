namespace Blossom.Monetization.IAP {
    using System;
    using System.Collections.Generic;
    using Common;
    using UnityEngine;
    
    public class IAPSettingsSO : SettingsSOBase {
        
        /// <summary>
        /// 등록된 IAP 상품 정의 목록.
        /// </summary>
        public IAPDefinition[] Definitions => definitions;
        /// <summary>
        /// 등록된 태그 정의 목록.
        /// </summary>
        public IReadOnlyList<IAPTagDefinition> TagDefinitions => tagDefinitions;
        
        [SerializeField] private IAPDefinition[] definitions;
        [SerializeField] private List<IAPTagDefinition> tagDefinitions = new();

        /// <summary>
        /// 태그 ID를 기준으로 태그 이름을 조회한다.
        /// </summary>
        /// <param name="id">조회할 태그 ID</param>
        /// <param name="tagName">조회 성공 시 태그 이름</param>
        /// <returns>조회 성공 시 true.</returns>
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

        /// <summary>
        /// Guid 문자열을 기준으로 태그 이름을 조회한다.
        /// </summary>
        /// <param name="guid">조회할 Guid 문자열</param>
        /// <param name="tagName">조회 성공 시 태그 이름</param>
        /// <returns>조회 성공 시 true.</returns>
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

        /// <summary>
        /// 태그 이름을 기준으로 대응되는 태그 ID를 조회한다.
        /// </summary>
        /// <param name="tagName">조회할 태그 이름</param>
        /// <param name="tagId">조회 성공 시 태그 ID</param>
        /// <param name="ignoreCase">대소문자 무시 여부</param>
        /// <returns>조회 성공 시 true.</returns>
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