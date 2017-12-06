using System.IO;
using System.Numerics;

namespace CipherStone
{
    public static class ensureNonGreedy
    {
        public static IFormatter<T> EnsureNonGreedy<T>(this IFormatter<T> @this)
        {
            if (@this.isGreedyDeserialize)
                return new LengthFormatter<T>(@this);
            return @this;
        }
        public static IFormatter<T> EnsureNonGreedy<T>(this IFormatter<T> @this, int maxSizebytes)
        {
            if (!@this.isGreedyDeserialize)
                return @this;
            IFormatter<BigInteger> lenFormatter;
            if (maxSizebytes > sizeof(ulong))
                lenFormatter = new TerminateIntegerFormatter();
            else if (maxSizebytes > sizeof(uint))
                lenFormatter = new ULongFormatter().ToBigIntFormatter();
            else if (maxSizebytes > sizeof(ushort))
                lenFormatter = new UIntFormatter().ToBigIntFormatter();
            else if (maxSizebytes > sizeof(byte))
                lenFormatter = new UShortFormatter().ToBigIntFormatter();
            else
                lenFormatter = new ByteFormatter().ToBigIntFormatter();

            return new LengthFormatter<T>(@this, lenFormatter);
        }
    }
    public class LengthFormatter<T> : IFormatter<T>
    {
        private readonly IFormatter<T> _inner;
        private readonly IFormatter<BigInteger> _sizeSerializer;
        public LengthFormatter(IFormatter<T> inner, IFormatter<BigInteger> sizeSerializer = null)
        {
            _inner = inner;
            _sizeSerializer = sizeSerializer ?? new TerminateIntegerFormatter();
        }
        public T Deserialize(Stream source)
        {
            int len = (int)_sizeSerializer.Deserialize(source);
            source = new LimitedStream(source, len);
            return _inner.Deserialize(source);
        }
        public void Serialize(T o, Stream sink)
        {
            int len = _inner.SerializeSize(o);
            if (len < 0)
            {
                var arr = _inner.serialize(o);
                _sizeSerializer.Serialize(arr.Length, sink);
                sink.Write(arr, 0, arr.Length);
            }
            else
            {
                _sizeSerializer.Serialize(len, sink);
                _inner.Serialize(o, sink);
            }
        }
        public int SerializeSize(T o)
        {
            int leno, lens;
            if ((leno = _inner.SerializeSize(o)) == -1)
                return -1;
            if ((lens = _sizeSerializer.SerializeSize(leno)) == -1)
                return -1;
            return leno + lens;
        }
        public bool isGreedyDeserialize => false;
    }
}
