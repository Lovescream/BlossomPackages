using System;
using System.Threading;
using System.Threading.Tasks;

namespace Blossom.Networking.Online {
    public static class Online {

        /// <summary>
        /// 현재 온라인 연결 상태.
        /// </summary>
        public static bool IsConnected => OnlineSystem.IsConnected;

        /// <summary>
        /// 온라인 연결 상태를 확인.
        /// </summary>
        /// <param name="type">확인할 연결 유형</param>
        /// <param name="cbOnSucceeded">연결 성공 시 콜백</param>
        /// <param name="cbOnFailed">연결 실패 시 콜백</param>
        public static void Validate(OnlineValidationType type = OnlineValidationType.Default,
            Action cbOnSucceeded = null, Action cbOnFailed = null) {
            OnlineSystem.Validate(type, cbOnSucceeded, cbOnFailed);
        }

        /// <summary>
        /// 온라인 연결 상태를 비동기적으로 확인.
        /// </summary>
        /// <param name="type">확인할 연결 유형</param>
        /// <returns>연결 성공 여부</returns>
        public static async Task<bool> ValidateAsync(OnlineValidationType type = OnlineValidationType.Default) {
            return await OnlineSystem.ValidateAsync(type);
        }

        /// <summary>
        /// 지정된 시간 동안 온라인 연결을 시도.
        /// </summary>
        /// <param name="duration">최대 시도 시간(초)</param>
        /// <param name="type">시도할 연결 유형</param>
        /// <param name="cbOnSucceeded">연결 성공 시 콜백</param>
        /// <param name="cbOnFailed">연결 실패 시 콜백</param>
        public static void TryConnect(float duration, OnlineValidationType type = OnlineValidationType.Default,
            Action cbOnSucceeded = null, Action cbOnFailed = null) {
            OnlineSystem.TryConnect(duration, type, cbOnSucceeded, cbOnFailed);
        }

        /// <summary>
        /// 지정된 시간 동안 온라인 연결을 비동기적으로 시도.
        /// </summary>
        /// <param name="duration">최대 시도 시간(초)</param>
        /// <param name="type">시도할 연결 유형</param>
        /// <returns></returns>
        public static async Task<bool> TryConnectAsync(float duration,
            OnlineValidationType type = OnlineValidationType.Default) {
            return await OnlineSystem.TryConnectAsync(duration, type);
        }

        /// <summary>
        /// 온라인 연결 상태 추적을 시작.
        /// </summary>
        /// <param name="type">추적할 연결 유형</param>
        /// <returns>추적 작업 제어를 위한 CancellationTokenSource</returns>
        public static CancellationTokenSource StartTracking(OnlineValidationType type = OnlineValidationType.Default) {
            return OnlineSystem.StartTrackConnection(type);
        }

        /// <summary>
        /// 온라인 연결 상태 추적을 중지.
        /// </summary>
        public static void StopTracking() {
            OnlineSystem.StopTrackConnection();
        }

        /// <summary>
        /// 온라인 연결 상태 변경 이벤트 구독.
        /// </summary>
        public static void SubscribeOnStateChanged(Action<bool> cb) {
            OnlineSystem.OnConnectionStateChanged += cb;
        }

        /// <summary>
        /// 온라인 연결 상태 변경 이벤트 구독 해제.
        /// </summary>
        public static void UnsubscribeOnStateChanged(Action<bool> cb) {
            OnlineSystem.OnConnectionStateChanged -= cb;
        }

        /// <summary>
        /// 온라인 연결 성공 이벤트 구독.
        /// </summary>
        public static void SubscribeOnSucceeded(Action cb) {
            OnlineSystem.OnConnectionSucceeded += cb;
        }
        
        /// <summary>
        /// 온라인 연결 성공 이벤트 구독 해제.
        /// </summary>
        public static void UnsubscribeOnSucceeded(Action cb) {
            OnlineSystem.OnConnectionSucceeded -= cb;
        }
        
        /// <summary>
        /// 온라인 연결 실패 이벤트 구독.
        /// </summary>
        public static void SubscribeOnFailed(Action cb) {
            OnlineSystem.OnConnectionFailed += cb;
        }
        
        /// <summary>
        /// 온라인 연결 실패 이벤트 구독 해제.
        /// </summary>
        public static void UnsubscribeOnFailed(Action cb) {
            OnlineSystem.OnConnectionFailed -= cb;
        }

        /// <summary>
        /// 온라인 연결 오류 이벤트 구독.
        /// </summary>
        public static void SubscribeOnError(Action<Exception> cb) {
            OnlineSystem.OnError += cb;
        }

        /// <summary>
        /// 온라인 연결 오류 이벤트 구독 해제.
        /// </summary>
        public static void UnsubscribeOnError(Action<Exception> cb) {
            OnlineSystem.OnError -= cb;
        }

    }
}
