using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Blossom.Persistence.Akiv.Internal {
    internal static class AkivEncrypt {
        private const string Key = "m71a12x28p94r6e5";
        private static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(Key);

        public static byte[] Encrypt(byte[] rawData) {
            if (rawData == null || rawData.Length == 0) return Array.Empty<byte>();

            try {
                using var rijndael = new RijndaelManaged();
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.KeySize = 128;
                rijndael.BlockSize = 128;
                rijndael.Key = KeyBytes;
                rijndael.IV = KeyBytes;

                using MemoryStream memoryStream = new();
                using (CryptoStream cryptoStream = new(
                           memoryStream, rijndael.CreateEncryptor(), CryptoStreamMode.Write)) {
                    cryptoStream.Write(rawData, 0, rawData.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return memoryStream.ToArray();
            }
            catch (Exception e) {
                Nyo.Error($"EncryptBinary Error: {e.Message}");
                return null;
            }
        }

        public static byte[] Decrypt(byte[] encryptedData) {
            if (encryptedData == null || encryptedData.Length == 0) return Array.Empty<byte>();

            try {
                using var rijndael = new RijndaelManaged();
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;
                rijndael.KeySize = 128;
                rijndael.BlockSize = 128;
                rijndael.Key = KeyBytes;
                rijndael.IV = KeyBytes;

                using MemoryStream memoryStream = new();
                using (CryptoStream cryptoStream = new(
                           memoryStream, rijndael.CreateDecryptor(), CryptoStreamMode.Write)) {
                    cryptoStream.Write(encryptedData, 0, encryptedData.Length);
                    cryptoStream.FlushFinalBlock();
                }

                return memoryStream.ToArray();
            }
            catch (Exception e) {
                Nyo.Error($"DecryptBinary Error: {e.Message}");
                return null;
            }
        }
    }
}