namespace Blossom.Presentation.Receive {
    public readonly struct ReceiveSpawnContext {
        public ReceiveKey Key { get; }
        public ReceiveSpace Space { get; }
        public int Amount { get; }

        public ReceiveSpawnContext(ReceiveKey key, ReceiveSpace space, int amount) {
            Key = key;
            Space = space;
            Amount = amount;
        }
    }
}