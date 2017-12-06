using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using WhetStone.Looping;
using WhetStone.Streams;

namespace CipherStone
{
    public static class Encryption
    {
        public const int IV_LENGTH = 128 / 8, KEY_LENGTH = 256 / 8;
        public static SymmetricAlgorithm getAlgorithm(byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            return new AesManaged
            {
                Key = key,
                IV = iv,
                Padding = paddingMode,
                BlockSize = 16*8
            };
        }
        public static byte[] GenValidKey(IEnumerable<byte> original)
        {
            return Sha2Hashing.Hash(original).Take(KEY_LENGTH).ToArray(KEY_LENGTH);
        }
        public static void Encrypt(Stream sink, byte[] plainText, out byte[] key, out byte[] iv)
        {
            using (var r = new AesManaged())
            {
                r.GenerateKey();
                key = r.Key;
                r.GenerateIV();
                iv = r.IV;
            }
            Encrypt(sink, plainText, key, iv);
        }
        public static void Encrypt(Stream sink, byte[] plainText, byte[] key, out byte[] iv)
        {
            using (var r = new AesManaged())
            {
                r.GenerateIV();
                iv = r.IV;
            }
            Encrypt(sink, plainText, key, iv);
        }
        public static void Encrypt(Stream sink, byte[] plainText, byte[] key, byte[] iv)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));

            using (CryptoStream csEncrypt = EncryptStream(sink, key, iv))
            {
                csEncrypt.Write(plainText, 0, plainText.Length);
                csEncrypt.FlushFinalBlock();
            }
        }
        public static CryptoStream EncryptStream(Stream sink, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));
            
            using (var rijAlg = getAlgorithm(key,iv, paddingMode))
            {
                
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                
                return new CryptoStream(sink, encryptor, CryptoStreamMode.Write);
            }
        }
        public static CryptoStream EncryptStream(Stream sink, byte[] key, out byte[] iv, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            using (var r = new AesManaged())
            {
                r.GenerateIV();
                iv = r.IV;
            }
            return EncryptStream(sink, key, iv, paddingMode);
        }
        public static byte[] Encrypt(byte[] plainText, byte[] key, out byte[] iv)
        {
            using (var ms = new MemoryStream())
            {
                Encrypt(ms, plainText, key, out iv);
                return ms.ToArray();
            }
        }
        public static byte[] Encrypt(byte[] plainText, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream())
            {
                Encrypt(ms, plainText, key, iv);
                return ms.ToArray();
            }
        }
        public static byte[] Decrypt(Stream cipher, byte[] key, byte[] iv)
        {
            if (cipher == null || cipher.Length <= 0)
                throw new ArgumentNullException(nameof(cipher));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));
            using (CryptoStream csDecrypt = DecryptStream(cipher, key, iv))
            {
                return csDecrypt.ReadAll();
            }

        }
        public static CryptoStream DecryptStream(Stream cipher, byte[] key, byte[] iv, PaddingMode paddingMode = PaddingMode.Zeros)
        {
            // Check arguments. 
            if (cipher == null || cipher.Length <= 0)
                throw new ArgumentNullException(nameof(cipher));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (var rijAlg = getAlgorithm(key, iv, paddingMode))
            {

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                return new CryptoStream(cipher, decryptor, CryptoStreamMode.Read);
            }

        }
        public static byte[] Decrypt(byte[] cipher, byte[] key, byte[] iv)
        {
            using (var ms = new MemoryStream(cipher, false))
            {
                return Decrypt(ms, key, iv);
            }
        }
    }
}
