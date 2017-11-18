using System.Collections.Generic;
using System.Security.Cryptography;
using WhetStone;

namespace CipherStone
{
    public static class Sha2Hashing
    {
        public const int HASH_LENGTH = 512 / 8;
        public static byte[] Hash(IEnumerable<byte> input)
        {
            using (var alg = SHA512.Create())
            {
                alg.ComputeHash(input.AsArray());
                return alg.Hash;
            }
        }
    }
}