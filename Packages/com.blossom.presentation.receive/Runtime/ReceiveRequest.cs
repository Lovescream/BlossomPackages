namespace Blossom.Presentation.Receive {
    using System;
    using UnityEngine;
    
    public readonly struct ReceiveRequest {

        public ReceiveKey Key { get; }
        public int Amount { get; }
        public Vector3 StartPosition { get; }
        public ReceiveSpace Space { get; }
        public ReceiveArcDirection ArcDirection { get; }
        public Action<ReceiveKey, int> OnChunkArrived { get; }
        public Action OnCompleted { get; }

        public ReceiveRequest(ReceiveKey key, int amount, Vector3 startPosition, ReceiveSpace space,
            ReceiveArcDirection arcDirection, Action<ReceiveKey, int> onChunkArrived = null, Action onCompleted = null) {
            Key = key;
            Amount = amount;
            StartPosition = startPosition;
            Space = space;
            ArcDirection = arcDirection;
            OnChunkArrived = onChunkArrived;
            OnCompleted = onCompleted;
        }
        
    }
}