#if SDK_FIREBASE

using Cysharp.Threading.Tasks;
using Firebase;

namespace Blossom.Platform.Firebase.Internal {
    
    public interface IFirebaseModule {
        string ModuleName { get; }
        UniTask InitializeAsync(FirebaseApp app);
    }
}

#endif