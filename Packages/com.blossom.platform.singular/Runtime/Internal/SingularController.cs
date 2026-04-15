#if SDK_SINGULAR

using Singular;

namespace Blossom.Platform.Singular.Internal {
    internal static class SingularController {
        internal static void SetCustomUserId(string id) => SingularSDK.SetCustomUserId(id);
    }
}

#endif