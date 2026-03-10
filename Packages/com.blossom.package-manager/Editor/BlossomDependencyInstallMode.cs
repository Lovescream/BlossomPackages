namespace Blossom.PackageManager.Editor {
    internal enum BlossomDependencyInstallMode {
        CatalogPackage,   // Catalog에 포함된 Blossom 패키지.
        UnityPackage,     // 유니티에서 설치 가능한 패키지. (Ex: com.unity.nuget.newtonsoft-json)
        GitPackage,       // 깃으로 설치 가능한 패키지. (Ex: UniTask git URL)
        ScopedRegistry,   // Scoped registry 세팅이 필요한 패키지. (Ex: AppLovin)
        Manual            // 자동 설치를 권장하지 않거나 할 수 없는 경우. (Ex: DOTween)
    }
}