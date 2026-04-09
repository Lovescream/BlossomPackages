namespace Blossom.Presentation.Receive.Internal {
    internal sealed class DefaultReceiveTargetResolver : IReceiveTargetResolver {
        public bool TryResolve(ReceiveKey key, out IReceiveTarget target) {
            return ReceiveTargetRegistry.TryGet(key, out target);
        }
    }
}