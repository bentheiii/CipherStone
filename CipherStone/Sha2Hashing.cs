using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using WhetStone.Looping;

namespace CipherStone
{
    public static class Sha2Hashing
    {
        public const int HASH_LENGTH = 512 / 8;
        public static byte[] Hash(IEnumerable<byte> input)
        {
            using (var alg = SHA512.Create())
            {
                Stream s = new EnumerationStream(input);
                alg.ComputeHash(s);
                return alg.Hash;
            }
        }
    }
}