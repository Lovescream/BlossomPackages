#if SDK_FIREBASE

using Firebase;

namespace Blossom.Platform.Firebase.Internal {
    using Cysharp.Threading.Tasks;
    
    public interface IFirebaseModule {
        string ModuleName { get; }
        UniTask InitializeAsync(FirebaseApp app);
    }
}

#endif