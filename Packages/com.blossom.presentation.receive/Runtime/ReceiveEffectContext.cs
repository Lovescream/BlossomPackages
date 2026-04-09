using System;
using UnityEngine;

namespace Blossom.Presentation.Receive {
    public sealed class ReceiveEffectContext {
        public IReceiveObject Object;
        public Vector3 StartPosition;
        public Vector3 TargetPosition;
        public ReceiveArcDirection Direction;
        public int Amount;
        public Action OnArrived;
    }
}