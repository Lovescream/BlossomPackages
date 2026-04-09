namespace Blossom.Presentation.Receive.Internal {
    internal sealed class DefaultReceiveEffectSelector : IReceiveEffectSelector {

        private readonly IReceiveEffect _effect;

        internal DefaultReceiveEffectSelector(IReceiveEffect effect) {
            _effect = effect;
        }

        public IReceiveEffect Select(ReceiveRequest request) {
            return _effect;
        }

    }
}