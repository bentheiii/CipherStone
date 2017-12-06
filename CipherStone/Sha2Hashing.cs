using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using WhetStone;

namespace CipherStone
{
    public static class Sha2Hashing
    {
        public static byte[] Hash(IEnumerable<byte> input)
        {
            using (var alg = SHA512.Create())
            {
                alg.ComputeHash(input.AsArray());
                return alg.Hash;
            }
        }
        public static int TransformBlock(this HashAlgorithm @this, byte[] bytes, int offset = 0, int count = -1)
        {
            if (count == -1)
                count = bytes.Length - offset;
            return @this.TransformBlock(bytes, offset, count, bytes, offset);
        }
        public static int TransformBlock(this HashAlgorithm @this, byte b)
        {
            var buffer = new []{b};
            return @this.TransformBlock(buffer);
        }
        public static void TransformFinalBlock(this HashAlgorithm @this)
        {
            var buffer = new byte[0];
            @this.TransformFinalBlock(buffer, 0, 0);
        }
    }
}