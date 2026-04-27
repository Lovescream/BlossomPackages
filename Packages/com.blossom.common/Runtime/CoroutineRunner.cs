namespace Blossom.Common {
    using UnityEngine;
    using System.Collections;
    
    public class CoroutineRunner : MonoBehaviour {

        public static CoroutineRunner Instance {
            get {
                if (_instance == null) {
                    GameObject obj = new("[Blossom:CoroutineRunner]");
                    Object.DontDestroyOnLoad(obj);
                    _instance = obj.AddComponent<CoroutineRunner>();
                }

                return _instance;
            }
        }
        
        private static CoroutineRunner _instance;

        public static Coroutine Run(IEnumerator routine) {
            return Instance.StartCoroutine(routine);
        }

        public static void Stop(Coroutine coroutine) {
            Instance.StopCoroutine(coroutine);
        }

    }
}