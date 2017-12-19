using System;
using System.IO;
using System.Numerics;
using WhetStone.SystemExtensions;

namespace CipherStone
{
    public class TerminateIntegerFormatter : IFormatter<BigInteger>
    {
        public TerminateIntegerFormatter(bool addSign = false)
        {
            this.addSign = addSign;
        }
        /*
        if sign is marked:
        the first byte being 255 means to negate the resultant
        so (under signed):
        1000->[255,235,3,255]
        256->[1,1,255]
        1->[1,255]
        0->[255,255]
        -1->[255,1,255]
        -5->[255,5,255]
        -256->[255,1,1,255]
        -1000->[255,235,3,255]
        */
        public bool addSign { get; }
        public void Serialize(BigInteger l, Stream sink)
        {
            if (l.Sign < 1)
            {
                if (addSign)
                {
                    sink.WriteByte(255);
                    l = -l;
                }
                else if (l.Sign == -1)
                {
                    throw new InvalidOperationException("cannot serialize negative biginteger");
                }
            }
            while (l != 0)
            {
                var b = (byte)(l % 255);
                sink.WriteByte(b);
                l /= 255;
            }
            sink.WriteByte(255);
        }
        public int SerializeSize(BigInteger o)
        {
            var addant = 0;
            if (o.Sign < 1)
            {
                if (addSign)
                {
                    addant = 1;
                    o = -o;
                }
                if (o.Sign == -1)
                {
                    return -1;
                }
            }

            return BigInteger.Log(o+1, 255).Ceil() + 1 + addant;
        }
        public bool isGreedyDeserialize => false;
        public BigInteger Deserialize(Stream source)
        {
            return deserialize(source, addSign);
        }
        private static BigInteger deserialize(Stream source, bool checkzero)
        {
            var ret = BigInteger.Zero;
            var coff = BigInteger.One;

            int digit;
            while ((digit = source.ReadByte()) != 255)
            {
                if (digit < 0)
                    throw new EndOfStreamException();
                ret += coff * digit;
                coff *= 255;
            }

            if (checkzero && ret.IsZero)
            {
                return -deserialize(source, false);
            }

            return ret;
        }
    }
}
