using System.IO;

namespace CipherStone
{
    public static class ensureNonGreedy
    {
        public static IByteSerializer<T> EnsureNonGreedy<T>(this IByteSerializer<T> @this)
        {
            if (@this.isGreedyDeserialize)
                return new LengthSerializer<T>(@this);
            return @this;
        }
    }
    public class LengthSerializer<T> : IByteSerializer<T>
    {
        private readonly IByteSerializer<T> _inner;
        private static void _serLength(int l, Stream sink)
        {
            while (l != 0)
            {
                var b = (byte)(l % 255);
                sink.WriteByte(b);
                l /= 255;
            }
            sink.WriteByte(255);
        }
        private static int _desLength(Stream source)
        {
            var ret = 0;
            var coff = 1;

            int digit;
            while ((digit = source.ReadByte()) != 255)
            {
                if (digit < 0)
                    throw new EndOfStreamException();
                ret += coff * digit;
                coff *= 255;
            }

            return ret;
        }
        private static int _sizeSerSize(int size)
        {
            int ret = 1;
            while (size != 0)
            {
                ret++;
                size /= 255;
            }
            return ret;
        }
        public LengthSerializer(IByteSerializer<T> inner)
        {
            _inner = inner;
        }
        public T deserialize(Stream source)
        {
            int len = _desLength(source);
            source = new LimitedStream(source, len);
            return _inner.deserialize(source);
        }
        public void serialize(T o, Stream sink)
        {
            int len = _inner.serializeSize(o);
            if (len < 0)
            {
                var arr = _inner.serialize(o);
                _serLength(arr.Length, sink);
                sink.Write(arr, 0, arr.Length);
            }
            else
            {
                _serLength(len, sink);
                _inner.serialize(o, sink);
            }
        }
        public int serializeSize(T o)
        {
            int len;
            if ((len = _inner.serializeSize(o)) == -1)
                return -1;
            return len + _sizeSerSize(len);
        }
        public bool isGreedyDeserialize => false;
    }
}
