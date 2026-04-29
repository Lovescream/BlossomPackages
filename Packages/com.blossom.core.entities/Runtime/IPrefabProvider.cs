namespace Blossom.Core.Entities {
    using UnityEngine;
    
    public interface IPrefabProvider {
        GameObject Provide(string key);
    }
}