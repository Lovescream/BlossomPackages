#if UNITY_EDITOR && SDK_SINGULAR

namespace Blossom.Platform.Singular.Editor {
    using UnityEditor;
    using Blossom.Common.Editor;
    
    public class SingularSettingsEditor : Editor {

        #region Const.

        private const string AssetFileName = "SingularSettings.asset";
        private const string SettingsMenuPath = "Blossom/Settings/Platform/SingularSettings";
        private const string CreateMenuPath = "Assets/Create/@Blossom/Platform/Singular Settings";

        #endregion

        #region Menu

        [MenuItem(SettingsMenuPath)]
        public static void SelectOrCreateSettings() {
            SettingsAssetUtility.SelectOrCreateAtRoot<SingularSettingsSO>(AssetFileName);
        }

        [MenuItem(CreateMenuPath)]
        private static void CreateSettingsAsset() {
            SettingsAssetUtility.SelectOrCreateAtRoot<SingularSettingsSO>(AssetFileName);
        }

        #endregion

    }
}

#endif