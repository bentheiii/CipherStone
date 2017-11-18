using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using WhetStone.Looping;
using WhetStone.Random;
using WhetStone.Streams;

namespace CipherStone
{
    public interface IByteSerializer
    {}
    public interface IByteSerializer<T> : IByteSerializer
    {
        T deserialize(Stream source);
        void serialize(T o, Stream sink);
        int serializeSize(T o);//-1 means extracting the size is the same as just deserializing the object
        bool isGreedyDeserialize { get; }
    }
    public static class ByteSerializerExtentions
    {
        public static T deserialize<T>(this IByteSerializer<T> @this, byte[] source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            MemoryStream memStream = new MemoryStream();
            memStream.Write(source, 0, source.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return @this.deserialize(memStream);
        }
        public static byte[] serialize<T>(this IByteSerializer<T> @this, T o)
        {
            var size = @this.serializeSize(o);
            MemoryStream ms;
            if (size < 0)
                ms = new MemoryStream();
            else
                ms = new MemoryStream(size);
            using (ms)
            {
                @this.serialize(o, ms);
                ms.Capacity = (int)ms.Length;
                return ms.GetBuffer();
            }
        }
    }
    public abstract class CastableSerializer : IByteSerializer
    {
        public IByteSerializer<O> cast<O>()
        {
            return (IByteSerializer<O>)this;
        }
    }
    public class DotNetSerializer<T> : IByteSerializer<T>
    {
        public readonly BinaryFormatter binaryFormatter = new BinaryFormatter
            { AssemblyFormat = FormatterAssemblyStyle.Simple};
        public T deserialize(Stream source)
        {
            return (T)binaryFormatter.Deserialize(source);
        }
        public void serialize(T o, Stream sink)
        {
            binaryFormatter.Serialize(sink, o);
        }
        public int serializeSize(T o) => -1;
        public bool isGreedyDeserialize => false;
    }
    #region primitives
    public abstract class ConstSizeSerializer<T> : CastableSerializer, IByteSerializer<T>, IByteSerializer<T[]>
    {
        protected abstract int elementSize { get; }
        protected abstract T deserializeElement(Stream source);
        protected abstract void serializeElement(T obj, Stream sink);

        public virtual T deserialize(Stream source)
        {
            if (source.Length < elementSize)
                throw new ArgumentException("array size invalid",nameof(source));
            return deserializeElement(source);
        }
        public virtual void serialize(T o, Stream sink)
        {
            serializeElement(o, sink);
        }
        public int serializeSize(T o) => elementSize;
        public bool isGreedyDeserialize => false;
        protected virtual void serializeArr(T[] o, Stream sink)
        {
            foreach (var t in o)
            {
                serialize(t,sink);
            }
        }
        void IByteSerializer<T[]>.serialize(T[] o, Stream sink)
        {
            serializeArr(o, sink);
        }
        public int serializeSize(T[] o)
        {
            return elementSize * o.Length;
        }
        bool IByteSerializer<T[]>.isGreedyDeserialize => true;
        protected virtual T[] deserializeArr(Stream source)
        {
            var ret = new T[source.Length / elementSize];
            foreach (var i in ret.Indices())
            {
                ret[i] = deserializeElement(source);
            }
            return ret;
        }
        T[] IByteSerializer<T[]>.deserialize(Stream source)
        {
            return deserializeArr(source);
        }
    }
    public class ByteSerializer : ConstSizeSerializer<byte>
    {
        protected override int elementSize => sizeof(byte);
        protected override byte deserializeElement(Stream source)
        {
            var ret = source.ReadByte();
            if (ret == -1)
                throw new EndOfStreamException();
            return (byte)ret;
        }
        protected override void serializeElement(byte obj, Stream sink)
        {
            sink.WriteByte(obj);
        }
        protected override void serializeArr(byte[] o, Stream sink)
        {
            sink.Write(o,0,o.Length);
        }
        protected override byte[] deserializeArr(Stream source)
        {
            return source.ReadAll();
        }
    }
    public abstract class BufferedConstSizeSerializer<T> : ConstSizeSerializer<T>
    {
        protected override int elementSize { get; }
        private readonly byte[] _buffer;
        protected BufferedConstSizeSerializer(int elementSize) : base()
        {
            this.elementSize = elementSize;
            _buffer = new byte[elementSize];
        }
        protected abstract T fromArr(byte[] arr);
        protected override T deserializeElement(Stream source)
        {
            if (source.Read(_buffer, 0, _buffer.Length) < _buffer.Length)
                throw new EndOfStreamException();
            return fromArr(_buffer);
        }
    }
    public class ShortSerializer : BufferedConstSizeSerializer<short>
    {
        public ShortSerializer() : base(sizeof(short)) { }
        protected override short fromArr(byte[] arr)
        {
            return BitConverter.ToInt16(arr,0);
        }
        protected override void serializeElement(short obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj),0,this.elementSize);
        }
    }
    public class IntSerializer : BufferedConstSizeSerializer<int>
    {
        public IntSerializer() : base(sizeof(int)) { }
        protected override int fromArr(byte[] arr)
        {
            return BitConverter.ToInt32(arr, 0);
        }
        protected override void serializeElement(int obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class LongSerializer : BufferedConstSizeSerializer<long>
    {
        public LongSerializer() : base(sizeof(long)) { }
        protected override long fromArr(byte[] arr)
        {
            return BitConverter.ToInt64(arr, 0);
        }
        protected override void serializeElement(long obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class ULongSerializer : BufferedConstSizeSerializer<ulong>
    {
        public ULongSerializer() : base(sizeof(ulong)) { }
        protected override ulong fromArr(byte[] arr)
        {
            return BitConverter.ToUInt64(arr, 0);
        }
        protected override void serializeElement(ulong obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class FloatSerializer : BufferedConstSizeSerializer<float>
    {
        public FloatSerializer() : base(sizeof(float)) { }
        protected override float fromArr(byte[] arr)
        {
            return BitConverter.ToSingle(arr, 0);
        }
        protected override void serializeElement(float obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class DoubleSerializer : BufferedConstSizeSerializer<double>
    {
        public DoubleSerializer() : base(sizeof(double)) { }
        protected override double fromArr(byte[] arr)
        {
            return BitConverter.ToDouble(arr, 0);
        }
        protected override void serializeElement(double obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class DecimalSerializer : BufferedConstSizeSerializer<decimal>
    {
        public DecimalSerializer() : base(sizeof(decimal)) { }
        protected override decimal fromArr(byte[] arr)
        {
            var ints = arr.Cast<int>().ToArray(arr.Length);
            return new decimal(ints);
        }
        protected override void serializeElement(decimal obj, Stream sink)
        {
            var arr = decimal.GetBits(obj).Cast<byte>().ToArray(this.elementSize);
            sink.Write(arr, 0, this.elementSize);
        }
    }
    #endregion
    public class StringSerializer : CastableSerializer,IByteSerializer<string>
    {
        public StringSerializer(Encoding encoder = null)
        {
            this.encoder = encoder ?? Encoding.UTF8;
        }
        public Encoding encoder { get; }
        public string deserialize(Stream source)
        {
            return encoder.GetString(source.ReadAll());
        }
        public void serialize(string o, Stream sink)
        {
            var bytes = encoder.GetBytes(o);
            sink.Write(bytes,0,bytes.Length);
        }
        public int serializeSize(string o)
        {
            return encoder.GetByteCount(o);
        }
        public bool isGreedyDeserialize => true;
    }
    public class EncryptedSerializer<T> : IByteSerializer<T>
    {
        private readonly IByteSerializer<T> _inner;
        private readonly byte[] _key;
        private readonly (int minInc,int maxExc) _paddingRange;
        public EncryptedSerializer(IByteSerializer<T> inner, byte[] key, (int, int)? paddingRange = null )
        {
            _inner = inner;
            _key = key;
            _paddingRange = paddingRange ?? (0,1);
        }
        private int _getPadding()
        {
            return new GlobalRandomGenerator().Int(_paddingRange.minInc, _paddingRange.maxExc);
        }
        public T deserialize(Stream source)
        {
            return _inner.deserialize(SecureEncryption.Decrypt(source.ReadAll(), _key));
        }
        public void serialize(T o, Stream sink)
        {
            var padding = _getPadding();
            var arr = SecureEncryption.Encrypt(_inner.serialize(o), _key, padding);
            sink.Write(arr,0,arr.Length);
        }
        public int serializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => true;
    }
    public class CompressedSerializer<T> : IByteSerializer<T>
    {
        private readonly IByteSerializer<T> _inner;
        public CompressedSerializer(IByteSerializer<T> inner)
        {
            _inner = inner;
        }
        public T deserialize(Stream source)
        {
            return _inner.deserialize(source.ReadAll().Compress());
        }
        public void serialize(T o, Stream sink)
        {
            var arr = _inner.serialize(o).Decompress();
            sink.Write(arr,0,arr.Length);
        }
        public int serializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => true;
    }
}
