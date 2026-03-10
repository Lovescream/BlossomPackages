using System.Collections;
using UnityEngine.SceneManagement;

namespace Blossom.Core.Scenes {
    /// <summary>
    /// 씬 전환 시점에 프로젝트가 개입할 수 있도록 하는 핸들러.
    /// 로딩 UI 표시, 매니저 정리, 특정 씬 유지 여부 등을 담당할 수 있음.
    /// </summary>
    public interface ISceneTransitionHandler {

        /// <summary>
        /// 씬 전환이 시작되기 직전에 호출됨.
        /// 로딩 UI 표시, 게임 정지, 매니저 정리 등의 작업에 사용.
        /// </summary>
        /// <param name="fromSceneName">현재 씬 이름</param>
        /// <param name="toSceneName">이동할 씬 이름</param>
        /// <returns></returns>
        IEnumerator OnBeforeSceneChange(string fromSceneName, string toSceneName);

        /// <summary>
        /// 새 씬이 로드된 직후 호출됨.
        /// 아직 이전 씬은 언로드되지 않았을 수 있음.
        /// </summary>
        /// <param name="loadedScene">방금 로드된 씬</param>
        /// <returns></returns>
        IEnumerator OnAfterSceneLoaded(Scene loadedScene);

        /// <summary>
        /// 씬 전환이 완전히 끝난 뒤 호출됨.
        /// 로딩 UI 숨김, 후처리 등에 사용.
        /// </summary>
        /// <param name="currentScene"></param>
        /// <returns></returns>
        IEnumerator OnAfterSceneChanged(Scene currentScene);

        /// <summary>
        /// 씬 전환 시 해당 씬을 언로드할지 여부를 결정. false를 반환하면 유지됨.
        /// </summary>
        /// <param name="sceneName">판단할 씬 이름</param>
        /// <returns>언로드할 경우 true</returns>
        bool ShouldUnloadScene(string sceneName);

    }
}