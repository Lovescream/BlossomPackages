using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blossom.Core.Scenes {
    internal static class SceneController {

        #region Properties
        
        internal static SceneBase Current { get; set; }

        #endregion
        
        #region Fields

        private static ISceneTransitionHandler _transitionHandler;

        #endregion

        #region Initialize

        internal static void Initialize(ISceneTransitionHandler handler = null) {
            _transitionHandler = handler;
        }

        internal static void SetHandler(ISceneTransitionHandler handler) {
            _transitionHandler = handler;
        }

        #endregion

        #region Load

        internal static void Load(string sceneName) {
            if (string.IsNullOrEmpty(sceneName)) {
                Debug.LogError($"[Blossom:Scene] Load({sceneName}): SceneName is null or empty.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }

        internal static void Reload() {
            Scene current = SceneManager.GetActiveScene();
            if (!current.IsValid()) {
                Debug.LogError($"[Blossom:Scene] Reload(): Active scene is invalid.");
                return;
            }

            SceneManager.LoadScene(current.buildIndex);
        }

        #endregion

        #region LoadAsync

        internal static Coroutine SwitchAsync(MonoBehaviour runner, string sceneName, Action<float> onProgress = null) {
            if (runner == null) {
                Debug.LogError($"[Blossom:Scene] SwitchAsync({sceneName}): Runner is null.");
                return null;
            }

            return runner.StartCoroutine(CoSwitchSceneAsync(sceneName, onProgress));
        }

        internal static Coroutine LoadAsync(MonoBehaviour runner, string sceneName, Action<float> onProgress = null) {
            if (runner == null) {
                Debug.LogError($"[Blossom:Scene] LoadAsync({sceneName}): Runner is null.");
                return null;
            }

            return runner.StartCoroutine(CoLoadSceneAsync(sceneName, onProgress));
        }

        private static IEnumerator CoLoadSceneAsync(string sceneName, Action<float> onProgress = null) {
            if (string.IsNullOrEmpty(sceneName)) {
                Debug.LogError($"[Blossom:Scene] CoLoadSceneAsync({sceneName}): SceneName is null or empty.");
                yield break;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (operation == null) {
                Debug.LogError($"[Blossom:Scene] CoLoadSceneAsync({sceneName}): Failed to start async load.");
                yield break;
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f) {
                onProgress?.Invoke(operation.progress / 0.9f);
                yield return null;
            }

            onProgress?.Invoke(1f);

            void OnLoaded(Scene scene, LoadSceneMode mode) {
                if (scene.name == sceneName) return;
                SceneManager.SetActiveScene(scene);
                SceneManager.sceneLoaded -= OnLoaded;
            }

            SceneManager.sceneLoaded += OnLoaded;
            operation.allowSceneActivation = true;

            while (!operation.isDone) yield return null;
        }

        private static IEnumerator CoSwitchSceneAsync(string sceneName, Action<float> onProgress = null) {
            if (string.IsNullOrEmpty(sceneName)) {
                Debug.LogError($"[Blossom:Scene] CoSwitchSceneAsync({sceneName}): SceneName is null or empty.");
                yield break;
            }

            Scene fromScene = SceneManager.GetActiveScene();
            if (_transitionHandler != null) yield return _transitionHandler.OnBeforeSceneChange(fromScene.name, sceneName);

            yield return CoLoadSceneAsync(sceneName, onProgress);

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid()) {
                SceneManager.SetActiveScene(loadedScene);
                if (_transitionHandler != null) yield return _transitionHandler.OnAfterSceneLoaded(loadedScene);
            }

            for (int i = SceneManager.sceneCount - 1; i >= 0; i--) {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneName) continue;
                if (_transitionHandler != null && !_transitionHandler.ShouldUnloadScene(scene.name)) continue;
                yield return SceneManager.UnloadSceneAsync(scene);
            }

            Scene current = SceneManager.GetActiveScene();
            if (_transitionHandler != null) yield return _transitionHandler.OnAfterSceneLoaded(current);
        }

        #endregion

    }
}