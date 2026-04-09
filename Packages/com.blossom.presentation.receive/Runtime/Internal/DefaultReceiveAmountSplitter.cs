using System;
using System.Collections.Generic;

namespace Blossom.Presentation.Receive.Internal {
    internal sealed class DefaultReceiveAmountSplitter : IReceiveAmountSplitter {
        public List<int> Split(ReceiveKey key, int amount) {
            List<int> result = new();

            int remaining = Math.Max(0, amount);
            while (remaining >= 5) {
                result.Add(5);
                remaining -= 5;
            }

            while (remaining > 0) {
                result.Add(1);
                remaining--;
            }

            return result;
        }
    }
}