namespace Blossom.Presentation.Receive.Internal {
    using UnityEngine;
    
    internal static class ReceiveSpaceUtility {
        internal static Vector3 Convert(Vector3 position, ReceiveSpace from, ReceiveSpace to, Camera worldCamera,
            float worldDepth, Vector3? targetWorldPosition = null) {
            if (from == to) return position;

            Camera cam = worldCamera != null ? worldCamera : Camera.main;
            if (cam == null) return position;

            if (from == ReceiveSpace.World && to == ReceiveSpace.Screen) {
                Vector3 screen = cam.WorldToScreenPoint(position);
                screen.z = 0f;
                return screen;
            }

            if (from == ReceiveSpace.Screen && to == ReceiveSpace.World) {
                float depth = worldDepth;

                if (targetWorldPosition.HasValue) {
                    depth = Mathf.Abs(targetWorldPosition.Value.z - cam.transform.position.z);
                    if (depth <= 1e-4f) depth = worldDepth;
                }

                Vector3 screenPoint = new Vector3(position.x, position.y, depth);
                return cam.ScreenToWorldPoint(screenPoint);
            }

            return position;
        }
    }
}