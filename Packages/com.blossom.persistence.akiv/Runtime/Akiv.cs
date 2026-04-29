namespace Blossom.Persistence.Akiv {
    using System;
    using Internal;
    public static class Akiv {
        public static bool IsInitialized { get; private set; }
        public static event Action OnBeforeSave;

        #region Initialize
        
        public static void Initialize() {
            if (IsInitialized) return;
            AkivSystem.Initialize();
            IsInitialized = true;
        }
        
        public static void DeleteAll() => AkivSystem.DeleteAll();
        
        #endregion

        #region Get

        public static AkivData Get(string key) => AkivSystem.Get(key);

        public static T Get<T>() where T : AkivData, new() => AkivSystem.Get<T>();

        public static T Get<T>(string key) where T : AkivData, new() => AkivSystem.Get<T>(key);

        #endregion

        #region Save / Load

        public static void SaveAll(bool forceSave = false) => AkivSystem.SaveAll(forceSave);
        public static void Save(string key, bool forceSave = false) => AkivSystem.Save(key, forceSave);
        public static void LoadAll() => AkivSystem.LoadAll();
        public static void Load(string key) => AkivSystem.Load(key);
        
        #endregion
        
        #region Events
        
        public static void OnChanged(string key) => AkivSystem.OnChanged(key);
        public static void InvokeOnBeforeSave() => OnBeforeSave?.Invoke();
        public static void RegisterOnDataLoaded(Action<string> cb) => AkivSystem.OnDataLoaded += cb;
        public static void UnregisterOnDataLoaded(Action<string> cb) => AkivSystem.OnDataLoaded -= cb;
        
        #endregion
        
    }
}