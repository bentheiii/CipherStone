using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace CipherStone
{
    public static class Sha2Hashing
    {
        public const int HASH_LENGTH = 512 / 8;
        public static byte[] Hash(IEnumerable<byte> input)
        {
            using (var alg = SHA512.Create())
            {
                alg.ComputeHash(input.ToArray());
                return alg.Hash;
            }
        }
    }
}