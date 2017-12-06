using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using WhetStone.Looping;
using WhetStone.Streams;
using WhetStone.WordPlay;

namespace CipherStone
{
    public class BigIntFormatter : IFormatter<BigInteger>
    {
        public BigInteger Deserialize(Stream source)
        {
            return new BigInteger(source.ReadAll());
        }
        public void Serialize(BigInteger o, Stream sink)
        {
            var arr = o.ToByteArray();
            sink.Write(arr, 0, arr.Length);
        }
        public int SerializeSize(BigInteger o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => true;
    }
}