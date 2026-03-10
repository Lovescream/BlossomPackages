using System.Linq;
using UnityEngine;

namespace Blossom.Core {
    public static class Utilities {

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component {
            if (!obj.TryGetComponent<T>(out T component)) component = obj.AddComponent<T>();
            return component;
        }

        public static T FindChild<T>(this GameObject obj, string name = null) where T : Component {
            if (obj == null) return null;
            T[] components = obj.GetComponentsInChildren<T>(true);
            if (components.Length == 0) return null;
            if (string.IsNullOrEmpty(name)) return components[0];
            return components.FirstOrDefault(x => x.name == name);
        }

        public static T FindChildDirect<T>(this GameObject obj, string name = null) where T : Component {
            if (obj == null) return null;
            for (int i = 0; i < obj.transform.childCount; i++) {
                Transform t = obj.transform.GetChild(i);
                if (!string.IsNullOrEmpty(name) && t.name != name) continue;
                if (t.TryGetComponent(out T component))
                    return component;
            }

            return null;
        }

        public static GameObject FindChild(this GameObject obj, string name = null) {
            Transform transform = FindChild<Transform>(obj, name);
            if (transform == null) return null;
            return transform.gameObject;
        }

        public static GameObject FindChildDirect(this GameObject obj, string name = null) {
            Transform transform = FindChildDirect<Transform>(obj, name);
            if (transform == null) return null;
            return transform.gameObject;
        }

        public static void DestroyAllChildren(this Transform t) {
            for (int i = t.childCount - 1; i >= 0; i--) {
                Object.Destroy(t.GetChild(i).gameObject);
            }
        }
        
    }
}