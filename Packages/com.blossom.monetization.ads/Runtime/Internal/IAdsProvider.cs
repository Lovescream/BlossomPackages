using Cysharp.Threading.Tasks;

namespace Blossom.Monetization.Ads.Internal {
    internal interface IAdsProvider {
        AdProviderType ProviderType { get; }
        bool IsInitialized { get; }
        
        UniTask<bool> InitializeAsync(AdsSettingsSO settings);
        bool Supports(AdType adType);
        AdUnitBase GetAdUnit(AdType adType);
    }
}