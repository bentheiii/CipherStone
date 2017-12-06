using System;
using System.IO;
using WhetStone.Streams;

namespace CipherStone
{
    public static class reverse
    {
        public static IFormatter<T> Reverse<T>(this IFormatter<T> @this)
        {
            if (@this is IConstSizeFormatter<T> c)
            {
                return new ReverseConstSizeFormatter<T>(c);
            }
            return new ReverseFormatter<T>(@this);
        }

        public static IFormatter<T> EnforceEndianess<T>(this IFormatter<T> @this, bool littleEndian = true)
        {
            if (BitConverter.IsLittleEndian == littleEndian)
            {
                return @this;
            }
            return @this.Reverse();
        }
    }
    public class ReverseFormatter<T> : IFormatter<T>
    {
        public ReverseFormatter(IFormatter<T> inner)
        {
            this.inner = inner;
        }
        public IFormatter<T> inner { get; }
        public T Deserialize(Stream source)
        {
            var all = source.ReadAll();
            Array.Reverse(all);
            return inner.deserialize(all);
        }
        public void Serialize(T o, Stream sink)
        {
            var bytes = inner.serialize(o);
            Array.Reverse(bytes);
            sink.Write(bytes, 0, bytes.Length);
        }
        public int SerializeSize(T o)
        {
            return inner.SerializeSize(o);
        }
        public bool isGreedyDeserialize => true;
    }
    public class ReverseConstSizeFormatter<T> : IConstSizeFormatter<T>
    {
        public ReverseConstSizeFormatter(IConstSizeFormatter<T> inner)
        {
            this.inner = inner;
        }
        public IConstSizeFormatter<T> inner { get; }
        public T Deserialize(Stream source)
        {
            var all = new byte[inner.elementSize];
            if (source.Read(all,0,all.Length) != all.Length)
                throw new EndOfStreamException();
            Array.Reverse(all);
            return inner.deserialize(all);
        }
        public void Serialize(T o, Stream sink)
        {
            var bytes = inner.serialize(o);
            Array.Reverse(bytes);
            sink.Write(bytes, 0, bytes.Length);
        }
        public int SerializeSize(T o)
        {
            return inner.SerializeSize(o);
        }
        public bool isGreedyDeserialize => false;
        public int elementSize
        {
            get
            {
                return inner.elementSize;
            }
        }
    }
}
