using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WhetStone.Looping;
using WhetStone.SystemExtensions;

namespace CipherStone
{
    public class LimitedIntFormatter : IFormatter<BigInteger>
    {
        public int size { get; }
        private readonly BigInteger _tooBig;
        public LimitedIntFormatter(int size)
        {
            this.size = size;
            _tooBig = BigInteger.Pow(256,this.size);
        }
        public BigInteger Deserialize(Stream source)
        {
            var buff = new byte[size];
            if (source.Read(buff,0,size) != size)
                throw new EndOfStreamException();
            return new BigInteger(buff);
        }
        public void Serialize(BigInteger o, Stream sink)
        {
            var arr = o.ToByteArray();
            if (arr.Length > size)
                throw new ArgumentOutOfRangeException("object is too big");
            if (size > arr.Length)
            {
                var padding = fill.Fill(size - arr.Length, (byte)0);
                arr = arr.Concat(padding).ToArray(size);
            }
            sink.Write(arr, 0, arr.Length);
        }
        public int SerializeSize(BigInteger o) => o > _tooBig ? -1 : size;
        public bool isGreedyDeserialize => false;
        public BigInteger validModulo(BigInteger original)
        {
            return _tooBig % original;
        }
        public static int sizeNeeded(BigInteger options)
        {
            return BigInteger.Log(options, 256).Ceil();
        }
    }
}
