using System;
using System.Linq;
using WhetStone.Looping;
using WhetStone.Random;

namespace CipherStone
{
    public static class SecureEncryption
    {
        public const int ORIGINALSIZELENGTH = 16;
        /*secure cypher scheme:
        64 bytes-hash of (key + everything after this)
        16 bits-iv of encryption
        the rest is encrypted serilized plaintext length (16 bytes, serialized in base-256, max input size is 2^64)+plaintext+padding
        size overhead: 96 bytes + padding
        */
        public static byte[] Encrypt(byte[] plainText, byte[] key, int padding = 0, Func<byte> padGenerator = null)
        {
            if (padGenerator == null && padding != 0)
                padGenerator = GlobalRandomGenerator.ThreadLocal().Byte;
            if (key.Length != Encryption.KEY_LENGTH)
                key = Encryption.GenValidKey(key);
            var padded = padding == 0 ? plainText : plainText.Concat(fill.Fill(padding, padGenerator)).ToArray();
            var orglength = NumberSerialization.FullCodeSerializer.ToBytes(plainText.Length).ToArray();
            if (orglength.Length > ORIGINALSIZELENGTH)
                throw new ArgumentException("plaintext too long");
            orglength = orglength.Concat(fill.Fill(ORIGINALSIZELENGTH - orglength.Length, (byte)0)).ToArray(ORIGINALSIZELENGTH);
            byte[] iv;
            byte[] cypher = Encryption.Encrypt(orglength.Concat(padded).ToArray(), key, out iv);
            var hash = Sha2Hashing.Hash(key.Concat(iv).Concat(cypher));
            return hash.Concat(iv).Concat(cypher).ToArray(iv.Length + cypher.Length + hash.Length);
        }
        public static byte[] Decrypt(byte[] enc, byte[] key)
        {
            bool hashMatch;
            var ret = Decrypt(enc, key, out hashMatch);
            if (!hashMatch)
                throw new FormatException("Hash Mismatch");
            return ret;
        }
        public static byte[] Decrypt(byte[] enc, byte[] key, out bool hashMatch)
        {
            if (key.Length != Encryption.KEY_LENGTH)
                key = Encryption.GenValidKey(key);
            hashMatch = enc.Take(Sha2Hashing.HASH_LENGTH).SequenceEqual(Sha2Hashing.Hash(key.Concat(enc.Skip(Sha2Hashing.HASH_LENGTH))));
            var padded = Encryption.Decrypt(enc.Skip(Sha2Hashing.HASH_LENGTH + Encryption.IV_LENGTH).ToArray(), key,
                enc.Skip(Sha2Hashing.HASH_LENGTH).Take(Encryption.IV_LENGTH).ToArray(Encryption.IV_LENGTH));
            var orglen = (int)NumberSerialization.FullCodeSerializer.FromBytes(padded.Take(ORIGINALSIZELENGTH));
            return padded.Skip(ORIGINALSIZELENGTH).Take(orglen).ToArray(orglen);
        }
    }
}