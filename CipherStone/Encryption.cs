using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using WhetStone.Looping;
using WhetStone.Streams;

namespace CipherStone
{
    public static class Encryption
    {
        public const int IV_LENGTH = 128 / 8, KEY_LENGTH = 256 / 8;
        public static byte[] GenValidKey(byte[] original)
        {
            return Sha2Hashing.Hash(original).Take(KEY_LENGTH).ToArray(KEY_LENGTH);
        }
        public static byte[] Encrypt(byte[] plainText, byte[] key, out byte[] iv)
        {
            using (var r = new AesManaged())
            {
                r.GenerateIV();
                iv = r.IV;
            }
            return Encrypt(plainText, key, iv);
        }
        public static byte[] Encrypt(byte[] plainText, byte[] key, byte[] iv)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException(nameof(plainText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));
            byte[] encrypted;
            
            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (AesManaged rijAlg = new AesManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(plainText, 0, plainText.Length);
                        csEncrypt.FlushFinalBlock();
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream. 
            return encrypted;
        }
        public static byte[] Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException(nameof(cipherText));
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException(nameof(key));
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException(nameof(iv));

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (AesManaged rijAlg = new AesManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption. 
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        /*using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream 
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }*/
                        return csDecrypt.ReadAll();
                    }
                }

            }

        }
    }
}
