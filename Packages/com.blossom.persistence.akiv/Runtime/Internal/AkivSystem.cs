using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blossom.Persistence.Akiv.Internal {
    internal static class AkivSystem {

        #region Settings.

        public static readonly bool UseEncryption = true;
        public static readonly bool SaveUseThread = true;
        public static readonly bool ClearOnSaves = false;
        public static readonly float AutoSaveInterval = 0f;

        #endregion

        #region Fields

        private static readonly Dictionary<string, Type> TypeMap = new();
        private static readonly Dictionary<string, AkivAttribute> AttributeMap = new();
        private static readonly Dictionary<string, AkivData> AkivDataMap = new();
        private static readonly Dictionary<string, bool> SaveRequestMap = new();

        public static event Action<string> OnDataLoaded;

        #endregion

        #region Initialize

        public static void Initialize() {
            AkivIO.Initialize();
            InitializeAkivData();

            LoadAll();
            if (ClearOnSaves) ClearAllAkivData();

            GameObject gameObject = new("[SAVE CALLBACK RECEIVER]") {
                hideFlags = HideFlags.HideInHierarchy
            };
            Object.DontDestroyOnLoad(gameObject);
            UnityCallbackReceiver receiver = gameObject.AddComponent<UnityCallbackReceiver>();
            if (AutoSaveInterval > 0) receiver.StartCoroutine(CoAutoSave());
        }

        private static void InitializeAkivData() {
            TypeMap.Clear();
            AkivDataMap.Clear();
            AttributeMap.Clear();
            
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies) {
                Type[] types;
                try {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e) {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (Type type in types) {
                    if (type == null) continue;
                    if (type.IsAbstract) continue;
                    if (!typeof(AkivData).IsAssignableFrom(type)) continue;
                    AkivAttribute attribute = (AkivAttribute)Attribute.GetCustomAttribute(type, typeof(AkivAttribute));
                    if (attribute == null) continue;
                    string key = string.IsNullOrEmpty(attribute.Key) ? type.Name : attribute.Key;
                    TypeMap[key] = type;
                    AttributeMap[key] = attribute;                }
            }
            
            if (TypeMap.Count <= 0) Nyo.Warning("No akiv types found. Did you forget [Akiv]?");
        }

        #endregion

        #region Get

        public static AkivData Get(string key) {
            if (string.IsNullOrEmpty(key)) return null;
            if (AkivDataMap.TryGetValue(key, out AkivData data)) return data;
            Load(key);
            return AkivDataMap.TryGetValue(key, out data) ? data : null;
        }

        public static T Get<T>() where T : AkivData, new() {
            string key;
            AkivAttribute attribute = (AkivAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(AkivAttribute));
            if (attribute != null) key = string.IsNullOrEmpty(attribute.Key) ? typeof(T).Name : attribute.Key;
            else key = typeof(T).Name;
            
            if (AkivDataMap.TryGetValue(key, out AkivData data)) return data as T;
            Load(key);
            if (AkivDataMap.TryGetValue(key, out data)) return data as T;
            Nyo.Error($"{key} is not registered.");
            return null;
        }

        public static T Get<T>(string key) where T : AkivData, new() {
            AkivData data = Get(key);
            if (data == null) return null;

            if (data is T typed) return typed;
            
            Nyo.Error($"Akiv key({key}) is loaded but type mismatch. Loaded: {data.GetType().Name}, Expected: {typeof(T).Name}");
            return null;
        }

        internal static bool GetUseEncryption(string key) {
            return AttributeMap.TryGetValue(key, out AkivAttribute attribute) && attribute.UseEncryption;
        }

        #endregion

        #region Save / Load

        public static void SaveAll(bool forceSave = false) {
            foreach (string key in TypeMap.Keys) Save(key, forceSave);
        }

        public static void Save(string key, bool forceSave = false) {
            if (!AkivDataMap.TryGetValue(key, out AkivData data)) {
                Nyo.Error($"Data({key}) is not loaded.");
                return;
            }

            if (!forceSave && (!SaveRequestMap.TryGetValue(key, out bool request) || !request)) return;

            data.Flush();
            if (SaveUseThread) {
                Thread thread = new(() => AkivIO.Serialize(data));
                thread.Start();
            }
            else {
                AkivIO.Serialize(data);
            }

            SaveRequestMap[key] = false;
        }

        private static IEnumerator CoAutoSave() {
            WaitForSeconds wait = new(AutoSaveInterval);
            while (true) {
                yield return wait;
                SaveAll();
            }
        }

        public static void LoadAll() {
            foreach (string key in TypeMap.Keys) Load(key);
        }

        public static void Load(string key) {
             if (AkivDataMap.ContainsKey(key)) return;

             if (!TypeMap.TryGetValue(key, out Type type) || type == null) {
                 Nyo.Error($"Type({key}) is not registered. Add [Akiv] to the class.");
                 return;
             }

             MethodInfo method =
                 typeof(AkivIO).GetMethod(nameof(AkivIO.Deserialize), BindingFlags.Public | BindingFlags.Static);
             if (method == null) return;
             MethodInfo closedMethod = method.MakeGenericMethod(type);
             AkivData data = (AkivData)closedMethod.Invoke(null, new object[] { key });
             data.Key = key;
             AkivDataMap[key] = data;
             
             SaveRequestMap[key] = false;
             OnDataLoaded?.Invoke(key);
        }

        #endregion

        #region Clear

        public static void DeleteAll() {
            foreach (string key in TypeMap.Keys) AkivIO.DeleteFile(key);
        }
        
        private static void ClearAllAkivData() {
            foreach (string key in TypeMap.Keys) ClearAkivData(key);
        }

        private static void ClearAkivData(string key) {
            if (!AkivDataMap.TryGetValue(key, out AkivData data)) return;
            data.Clear();
            SaveRequestMap[key] = true;
            Nyo.Log($"{key} is cleared.");
        }

        #endregion

        #region Events

        public static void OnChanged(string key) {
            if (AkivDataMap.ContainsKey(key))
                SaveRequestMap[key] = true;
        }

        #endregion

        private class UnityCallbackReceiver : MonoBehaviour {
            
            private void OnDestroy() {
#if UNITY_EDITOR
                Akiv.InvokeOnBeforeSave();
                SaveAll(true);
#endif
            }

            private void OnApplicationFocus(bool hasFocus) {
#if !UNITY_EDITOR
                if (!hasFocus) {
                    Akiv.InvokeOnBeforeSave();
                    SaveAll();
                }
#endif
            }
        }
        
    }
}
