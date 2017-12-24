using System;
using System.IO;
using System.Numerics;
using NumberStone;
using WhetStone.Looping;

namespace CipherStone
{
    public class VarSizeIntFormatter : BufferedConstSizeFormatter<BigInteger>
    {
        public bool allowNegative { get; }
        public VarSizeIntFormatter(int byteCount, bool allowNegative) : base(byteCount)
        {
            this.allowNegative = allowNegative;
        }
        //max is exclusive
        public BigInteger Max
        {
            get
            {
                var ret = BigInteger.Pow(new BigInteger(256), elementSize);
                if (allowNegative)
                {
                    ret/=2;
                }
                return ret;
            }
        }
        //min is exclusive
        public BigInteger Min
        {
            get
            {
                if (!allowNegative)
                {
                    return -1;
                }
                return -Max;
            }
        }
        protected override void serializeElement(BigInteger obj, Stream sink)
        {
            if (!obj.iswithinexclusive(Min,Max))
                throw new ArgumentException("biginteger is too large");

            int bytesLeft = elementSize;
            if (allowNegative)
            {
                byte firstbyte = obj < 0 ? (byte)128 : (byte)0;
                obj = BigInteger.Abs(obj);
                firstbyte |= (byte)(obj % 128);
                obj /= 128;
                sink.WriteByte(firstbyte);
                bytesLeft--;
            }
            while (!obj.IsZero)
            {
                bytesLeft--;
                sink.WriteByte((byte)(obj % 256));
                obj /= 256;
            }
            while (bytesLeft != 0)
            {
                bytesLeft--;
                sink.WriteByte(0);
            }
        }
        protected override BigInteger fromArr(byte[] arr)
        {
            BigInteger ret = 0;
            BigInteger pow = 1;
            int startIndex = 0;
            bool negative = false;
            if (allowNegative)
            {
                negative = (arr[0] & 128) != 0;
                ret += arr[0] & 127;
                pow *= 128;
                startIndex++;
            }
            foreach (var i in arr.Skip(startIndex))
            {
                ret += i * pow;
                pow *= 256;
            }
            if (negative)
                ret *= -1;
            return ret;
        }
    }
}
