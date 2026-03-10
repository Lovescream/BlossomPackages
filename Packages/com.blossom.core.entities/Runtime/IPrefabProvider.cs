using UnityEngine;

namespace Blossom.Core.Entities {
    public interface IPrefabProvider {
        GameObject Provide(string key);
    }
}