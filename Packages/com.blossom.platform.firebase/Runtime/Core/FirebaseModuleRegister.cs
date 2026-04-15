#if SDK_FIREBASE

using Blossom.Platform.Firebase.Internal;

namespace Blossom.Platform.Firebase {
    public static class FirebaseModuleRegister {
        /// <summary>
        /// Firebase 모듈을 등록: 내부 호출용. 
        /// </summary>
        /// <param name="module"></param>
        public static void RegisterModule(IFirebaseModule module) => FirebaseInitializer.RegisterModule(module);
    }
}

#endif