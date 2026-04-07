using System;
using UnityEngine;

namespace Blossom.Core.Scenes {
    public static class Scenes {

        /// <summary>
        /// 현재 활성화된 SceneBase.
        /// </summary>
        public static SceneBase Current => SceneController.Current;

        /// <summary>
        /// 씬 시스템 초기화.
        /// </summary>
        /// <param name="handler">씬 전환 핸들러</param>
        public static void Initialize(ISceneTransitionHandler handler = null) => SceneController.Initialize(handler);

        /// <summary>
        /// 씬 전환 핸들러 교체.
        /// </summary>
        /// <param name="handler">새 씬 전환 핸들러</param>
        public static void SetHandler(ISceneTransitionHandler handler) => SceneController.SetHandler(handler);

        /// <summary>
        /// 지정한 씬을 즉시 로드.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        public static void Load(string sceneName) => SceneController.Load(sceneName);

        /// <summary>
        /// 현재 활성 씬을 다시 로드.
        /// </summary>
        public static void Reload() => SceneController.Reload();

        /// <summary>
        /// 지정한 씬을 비동기 로드.
        /// </summary>
        /// <param name="runner">코루틴 실행용 MonoBehaviour</param>
        /// <param name="sceneName">이동할 씬 이름</param>
        /// <param name="onProgress">로딩 진행률 콜백 (0-1)</param>
        /// <returns>시작된 코루틴. 실패 시 null</returns>
        public static Coroutine LoadAsync(MonoBehaviour runner, string sceneName, Action<float> onProgress = null)
            => SceneController.LoadAsync(runner, sceneName, onProgress);
        
        /// <summary>
        /// 지정한 씬으로 비동기 전환 시작.
        /// </summary>
        /// <param name="runner">코루틴 실행용 MonoBehaviour</param>
        /// <param name="sceneName">이동할 씬 이름</param>
        /// <param name="onProgress">로딩 진행률 콜백 (0-1)</param>
        /// <returns>시작된 코루틴. 실패 시 null</returns>
        public static Coroutine SwitchAsync(MonoBehaviour runner, string sceneName, Action<float> onProgress = null)
            => SceneController.SwitchAsync(runner, sceneName, onProgress);

    }
}