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
    public class BigIntSerializer : CastableSerializer, IByteSerializer<BigInteger>
    {
        public BigInteger deserialize(Stream source)
        {
            return new BigInteger(source.ReadAll());
        }
        public void serialize(BigInteger o, Stream sink)
        {
            var arr = o.ToByteArray();
            sink.Write(arr, 0, arr.Length);
        }
        public int serializeSize(BigInteger o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => true;
    }
}