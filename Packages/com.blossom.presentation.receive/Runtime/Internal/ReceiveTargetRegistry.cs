namespace Blossom.Presentation.Receive.Internal {
    using System.Collections.Generic;
    
    internal static class ReceiveTargetRegistry {
        // 활성 ReceiveTarget 레지스트리.
        // 같은 Key를 가진 대상이 여러 개라면, 가장 마지막 활성 대상을 우선 사용.

        private static readonly Dictionary<ReceiveKey, Stack<IReceiveTarget>> Targets = new();

        internal static void Register(IReceiveTarget target) {
            if (target == null || !target.Key.IsValid) return;
            if (!Targets.TryGetValue(target.Key, out Stack<IReceiveTarget> stack)) {
                stack = new();
                Targets.Add(target.Key, stack);
            }

            stack.Push(target);
        }

        internal static void Unregister(IReceiveTarget target) {
            if (target == null || !target.Key.IsValid) return;
            if (!Targets.TryGetValue(target.Key, out Stack<IReceiveTarget> stack)) return;

            Stack<IReceiveTarget> temp = new();
            while (stack.Count > 0) {
                IReceiveTarget current = stack.Pop();
                if (!ReferenceEquals(current, target)) temp.Push(current);
            }

            while (temp.Count > 0) stack.Push(temp.Pop());
            if (stack.Count == 0) Targets.Remove(target.Key);
        }

        internal static bool TryGet(ReceiveKey key, out IReceiveTarget target) {
            target = null;
            if (!Targets.TryGetValue(key, out Stack<IReceiveTarget> stack) || stack.Count <= 0) return false;
            target = stack.Peek();
            return target != null;
        }

    }
}