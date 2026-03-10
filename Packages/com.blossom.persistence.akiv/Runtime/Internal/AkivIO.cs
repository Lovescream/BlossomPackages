using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Blossom.Persistence.Akiv.Internal {
    internal static class AkivIO {

        #region Const.
        
        private static readonly JsonSerializerSettings JsonSettings = new() {
            Formatting = Formatting.Indented,
            ContractResolver = new UnitySerializeFieldContractResolver(),
            ObjectCreationHandling = ObjectCreationHandling.Replace, // 새 객체를 만들어 교체시킴.
            TypeNameHandling = TypeNameHandling.None // Json에 타입 이름을 저장하지 않음.
        };

        #endregion

        #region Properties
        
        private static string DataPath {
            get {
                if (string.IsNullOrEmpty(_persistentDataPath))
                    _persistentDataPath = Application.persistentDataPath;
                return _persistentDataPath;
            }
        }

        #endregion

        #region Fields

        private static string _persistentDataPath;

        #endregion
        
        public static void Initialize() { }

        public static bool Serialize(AkivData data) {
            string path = GetPath(data.Key);

            try {
                string json = JsonConvert.SerializeObject(data, JsonSettings);
                byte[] raw = Encoding.UTF8.GetBytes(json);

                if (AkivSystem.GetUseEncryption(data.Key)) {
                    byte[] encrypt = AkivEncrypt.Encrypt(raw);
                    if (encrypt == null) {
                        Nyo.Error($"Failed to encrypt file: {path}");
                        return false;
                    }

                    File.WriteAllBytes(path, encrypt);
                }
                else File.WriteAllBytes(path, raw);

                return true;
            }
            catch (Exception e) {
                Nyo.Error($"{data.GetType()} Serialize Error: {e.Message}");
                return false;
            }
        }

        public static T Deserialize<T>(string key) where T : AkivData, new() {
            string path = GetPath(key);
            if (!File.Exists(path)) {
                T newObject = new T { Key = key };
                return newObject;
            }

            byte[] bytes = File.ReadAllBytes(path);

            // Attribute 설정 기준으로 암호화 적용하여 역직렬화 시도.
            bool preferEncrypt = AkivSystem.GetUseEncryption(key);
            if (TryDeserialize(bytes, preferEncrypt, out T obj)) {
                obj.Key = key;
                return obj;
            }

            // 실패 시 암호화 여부 반대로 적용하여 역직렬화 시도.
            if (TryDeserialize(bytes, !preferEncrypt, out obj)) {
                obj.Key = key;
                return obj;
            }

            T fallback = new T { Key = key };
            return fallback;
        }

        private static bool TryDeserialize<T>(byte[] bytes, bool decrypt, out T obj) where T : AkivData {
            obj = null;

            try {
                if (decrypt) {
                    bytes = AkivEncrypt.Decrypt(bytes);
                    if (bytes == null || bytes.Length == 0) return false;
                }

                string json = Encoding.UTF8.GetString(bytes);
                obj = JsonConvert.DeserializeObject<T>(json, JsonSettings);
                return obj != null;
            }
            catch {
                return false;
            }
        }

        public static bool DeleteFile(string key) {
            try {
                string path = GetPath(key);
                Debug.Log(path);
                if (!File.Exists(path)) return false;
                File.Delete(path);
                return true;
            }
            catch (Exception e) {
                Nyo.Error($"Delete Error: {e.Message}");
                return false;
            }
        }

        private static string GetPath(string key) {
            string directory = Path.Combine(DataPath, "Akiv");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return Path.Combine(directory, $"{key}.json");
        }

    }

    internal sealed class UnitySerializeFieldContractResolver : DefaultContractResolver {

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            
            // #1. 기본 규칙으로 만든 목록 받기. (주로 public property)
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            
            // #2. 래퍼 타입 제외.
            if (typeof(AkivData).IsAssignableFrom(type)) {
                properties = properties.Where(p => !IsAkivWrapperProperty(type, p)).ToList();
            }
            
            // #2. 필드들을 직접 탐색하여 규칙에 맞는 필드 추가.
            foreach (FieldInfo field in type.GetFields(flags)) {
                // [NonSerialized] 제외하기.
                if (Attribute.IsDefined(field, typeof(NonSerializedAttribute))) continue;
                
                // [SerializeField] private 필드만 포함하기.
                if (!field.IsPublic && !Attribute.IsDefined(field, typeof(SerializeField))) continue;
                
                // 이 필드로 JsonProperty 생성.
                JsonProperty jsonProperty = CreateProperty(field, memberSerialization);
                jsonProperty.Readable = true;
                jsonProperty.Writable = true;
                
                // 이미 같은 이름의 property가 있으면 중복 추가 방지.
                if (properties.Any(p => p.PropertyName == jsonProperty.PropertyName)) continue;

                properties.Add(jsonProperty);
            }

            return properties;
        }

        private static bool IsAkivWrapperProperty(Type declaringType, JsonProperty property) {
            if (property.UnderlyingName == null) return false;
            PropertyInfo prop = declaringType.GetProperty(
                property.UnderlyingName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop == null) return false;
            
            return typeof(IAkivWrapper).IsAssignableFrom(prop.PropertyType);
        }

    }
}