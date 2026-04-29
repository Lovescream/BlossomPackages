namespace Blossom.Core.UI {
    using UnityEngine;
    
    public interface IPrefabProvider {
        public GameObject Provide(string key);
    }
}