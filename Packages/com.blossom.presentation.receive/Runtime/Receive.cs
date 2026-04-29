namespace Blossom.Presentation.Receive {
    using Internal;
    using UnityEngine;
    
    public static class Receive {

        public static Transform ReceiveUIParent => ReceiveSystem.ReceiveUIParent;
        
        public static bool IsInitialized => ReceiveSystem.IsInitialized;

        public static void Initialize(
            IReceiveRewardHandler rewardHandler,
            IReceiveTargetResolver targetResolver,
            IReceiveSpawner spawner,
            IReceiveIconProvider iconProvider,
            IReceiveAmountSplitter amountSplitter = null,
            IReceiveAmountTextFormatter amountTextFormatter = null,
            IReceiveEffectSelector effectSelector = null,
            ReceiveSettings settings = null) {
            ReceiveSystem.Initialize(rewardHandler, targetResolver, spawner, iconProvider, amountSplitter,
                amountTextFormatter, effectSelector, settings);
        }

        /// <summary>
        /// 연출 없이 즉시 지급.
        /// </summary>
        public static void Apply(ReceiveKey key, int amount) => ReceiveSystem.Apply(key, amount);

        /// <summary>
        /// 연출만 재생.
        /// </summary>
        public static void Play(ReceiveRequest request) => ReceiveSystem.Play(request);

        /// <summary>
        /// 지급 및 연출 재생.
        /// 실제 지급 시점은 request.ApplyTiming을 따른다.
        /// </summary>
        public static void ApplyAndPlay(ReceiveRequest request) => ReceiveSystem.ApplyAndPlay(request);

        /// <summary>
        /// Receive 시스템을 초기 상태로 되돌림.
        /// </summary>
        public static void Reset() => ReceiveSystem.Reset();

    }
}