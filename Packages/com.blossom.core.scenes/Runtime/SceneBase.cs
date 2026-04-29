namespace Blossom.Core.Scenes {
    using System.Collections;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    
    public abstract class SceneBase : MonoBehaviour {

        private bool _initialized;

        protected virtual void Start() {
            StartCoroutine(CoWaitActiveThenInitialize());
        }

        private IEnumerator CoWaitActiveThenInitialize() {
            Scene scene = this.gameObject.scene;
            yield return new WaitUntil(() => SceneManager.GetActiveScene() == scene);
            Initialize();
        }

        protected virtual bool Initialize() {
            if (_initialized) return false;
            _initialized = true;

            SceneController.Current = this;
            OnInitialize();

            return true;
        }

        protected virtual void OnInitialize() { }

    }
}