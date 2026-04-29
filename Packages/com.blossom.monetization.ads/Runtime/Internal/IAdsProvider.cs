namespace Blossom.Monetization.Ads.Internal {
    using Cysharp.Threading.Tasks;
    
    internal interface IAdsProvider {
        AdProviderType ProviderType { get; }
        bool IsInitialized { get; }
        
        UniTask<bool> InitializeAsync(AdsSettingsSO settings);
        bool Supports(AdType adType);
        AdUnitBase GetAdUnit(AdType adType);
    }
}