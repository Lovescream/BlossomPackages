using UnityEngine;

namespace Blossom.Core.UI {
    public interface IPrefabProvider {
        public GameObject Provide(string key);
    }
}