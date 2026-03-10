using System;
using UnityEngine;

namespace Blossom.Monetization.IAP {
    [Serializable]
    public class IAPTagDefinition {
        public string Guid => guid;
        public string Name => name;
        [SerializeField] private string guid;
        [SerializeField] private string name;

#if UNITY_EDITOR
        public static IAPTagDefinition Create(string name) {
            return new IAPTagDefinition {
                guid = System.Guid.NewGuid().ToString("N"),
                name = name?.Trim()
            };
        }

        public void SetName(string newName) => name = newName?.Trim();
#endif
    }
}