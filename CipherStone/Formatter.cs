using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using WhetStone.Looping;
using WhetStone.Streams;

namespace CipherStone
{
    public interface IGenericFormatter
    {}
    public interface IFormatter<T> : IGenericFormatter
    {
        bool isGreedyDeserialize { get; }
        T Deserialize(Stream source);
        void Serialize(T o, Stream sink);
        int SerializeSize(T o);//-1 means extracting the size is the same as just deserializing the object
    }
    public static class GenericFormatterExtentions
    {
        public static T deserialize<T>(this IFormatter<T> @this, byte[] source)
        {
            return @this.deserialize(source, 0, source.Length);
        }
        public static T deserialize<T>(this IFormatter<T> @this, byte[] source, int start, int length)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            MemoryStream memStream = new MemoryStream();
            memStream.Write(source, start, length);
            memStream.Seek(0, SeekOrigin.Begin);
            return @this.Deserialize(memStream);
        }
        public static byte[] serialize<T>(this IFormatter<T> @this, T o)
        {
            var size = @this.SerializeSize(o);
            MemoryStream ms;
            if (size < 0)
                ms = new MemoryStream();
            else
                ms = new MemoryStream(size);
            using (ms)
            {
                @this.Serialize(o, ms);
                ms.Capacity = (int)ms.Length;
                return ms.GetBuffer();
            }
        }
        public static void Write<T>(this Stream @this, T o, IFormatter<T> formatter = null)
        {
            formatter = formatter ?? getFormatter.GetFormatter<T>();
            formatter.Serialize(o, @this);
        }
        public static T Read<T>(this Stream @this, IFormatter<T> formatter = null)
        {
            formatter = formatter ?? getFormatter.GetFormatter<T>();
            return formatter.Deserialize(@this);
        }
        public static IFormatter<O> cast<O>(this IGenericFormatter @this)
        {
            return (IFormatter<O>)@this;
        }
    }
    public abstract class GenericFormatter<T> : IFormatter<T>
    {
        private readonly IFormatter _inner;
        protected GenericFormatter(IFormatter inner)
        {
            _inner = inner;
        }
        public T Deserialize(Stream source)
        {
            return (T)_inner.Deserialize(source);
        }
        public void Serialize(T o, Stream sink)
        {
            _inner.Serialize(sink, o);
        }
        public virtual int SerializeSize(T o) => -1;
        public abstract bool isGreedyDeserialize { get; }
    }
    public class DotNetFormatter<T> : GenericFormatter<T>
    {
        public DotNetFormatter() : base(new BinaryFormatter()){}
        public override bool isGreedyDeserialize => false;
    }
    #region primitives
    public interface IConstSizeFormatter<T> : IFormatter<T>
    {
        int elementSize { get; }
    }
    public abstract class ConstSizeFormatter<T> : IConstSizeFormatter<T>, IFormatter<T[]>
    {
        public abstract int elementSize { get; }
        protected abstract T deserializeElement(Stream source);
        protected abstract void serializeElement(T obj, Stream sink);

        public virtual T Deserialize(Stream source)
        {
            if (source.Length < elementSize)
                throw new ArgumentException("array size invalid",nameof(source));
            return deserializeElement(source);
        }
        public virtual void Serialize(T o, Stream sink)
        {
            serializeElement(o, sink);
        }
        public int SerializeSize(T o) => elementSize;
        public bool isGreedyDeserialize => false;
        protected virtual void serializeArr(T[] o, Stream sink)
        {
            foreach (var t in o)
            {
                Serialize(t,sink);
            }
        }
        void IFormatter<T[]>.Serialize(T[] o, Stream sink)
        {
            serializeArr(o, sink);
        }
        public int SerializeSize(T[] o)
        {
            return elementSize * o.Length;
        }
        bool IFormatter<T[]>.isGreedyDeserialize => true;
        protected virtual T[] deserializeArr(Stream source)
        {
            var ret = new T[source.Length / elementSize];
            foreach (var i in ret.Indices())
            {
                ret[i] = deserializeElement(source);
            }
            return ret;
        }
        T[] IFormatter<T[]>.Deserialize(Stream source)
        {
            return deserializeArr(source);
        }
    }
    public class ByteFormatter : ConstSizeFormatter<byte>
    {
        public override int elementSize => sizeof(byte);
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
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, byte>(this, a => (byte)a, a => new BigInteger(a), true);
        }
    }
    public class SByteFormatter : ConstSizeFormatter<sbyte>
    {
        public override int elementSize => sizeof(sbyte);
        protected override sbyte deserializeElement(Stream source)
        {
            var ret = source.ReadByte();
            if (ret == -1)
                throw new EndOfStreamException();
            return (sbyte)ret;
        }
        protected override void serializeElement(sbyte obj, Stream sink)
        {
            sink.WriteByte((byte)obj);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, sbyte>(this, a => (sbyte)a, a => new BigInteger(a), true);
        }
    }
    public abstract class BufferedConstSizeFormatter<T> : ConstSizeFormatter<T>
    {
        public override int elementSize { get; }
        private readonly byte[] _buffer;
        protected BufferedConstSizeFormatter(int elementSize)
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
    public class ShortFormatter : BufferedConstSizeFormatter<short>
    {
        public ShortFormatter() : base(sizeof(short)) { }
        protected override short fromArr(byte[] arr)
        {
            return BitConverter.ToInt16(arr,0);
        }
        protected override void serializeElement(short obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj),0,this.elementSize);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger,short>(this, a=>(short)a, a=>new BigInteger(a),true);
        }
    }
    public class UShortFormatter : BufferedConstSizeFormatter<ushort>
    {
        public UShortFormatter() : base(sizeof(ushort)) { }
        protected override ushort fromArr(byte[] arr)
        {
            return BitConverter.ToUInt16(arr, 0);
        }
        protected override void serializeElement(ushort obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, ushort>(this, a => (ushort)a, a => new BigInteger(a), true);
        }
    }
    public class IntFormatter : BufferedConstSizeFormatter<int>
    {
        public IntFormatter() : base(sizeof(int)) { }
        protected override int fromArr(byte[] arr)
        {
            return BitConverter.ToInt32(arr, 0);
        }
        protected override void serializeElement(int obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, int>(this, a => (int)a, a => new BigInteger(a), true);
        }
    }
    public class UIntFormatter : BufferedConstSizeFormatter<uint>
    {
        public UIntFormatter() : base(sizeof(uint)) { }
        protected override uint fromArr(byte[] arr)
        {
            return BitConverter.ToUInt32(arr, 0);
        }
        protected override void serializeElement(uint obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, uint>(this, a => (uint)a, a => new BigInteger(a), true);
        }
    }
    public class LongFormatter : BufferedConstSizeFormatter<long>
    {
        public LongFormatter() : base(sizeof(long)) { }
        protected override long fromArr(byte[] arr)
        {
            return BitConverter.ToInt64(arr, 0);
        }
        protected override void serializeElement(long obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, long>(this, a => (long)a, a => new BigInteger(a), true);
        }
    }
    public class ULongFormatter : BufferedConstSizeFormatter<ulong>
    {
        public ULongFormatter() : base(sizeof(ulong)) { }
        protected override ulong fromArr(byte[] arr)
        {
            return BitConverter.ToUInt64(arr, 0);
        }
        protected override void serializeElement(ulong obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
        public IFormatter<BigInteger> ToBigIntFormatter()
        {
            return new SelectFuncFormatter<BigInteger, ulong>(this, a => (ulong)a, a => new BigInteger(a), true);
        }
    }
    public class FloatFormatter : BufferedConstSizeFormatter<float>
    {
        public FloatFormatter() : base(sizeof(float)) { }
        protected override float fromArr(byte[] arr)
        {
            return BitConverter.ToSingle(arr, 0);
        }
        protected override void serializeElement(float obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class DoubleFormatter : BufferedConstSizeFormatter<double>
    {
        public DoubleFormatter() : base(sizeof(double)) { }
        protected override double fromArr(byte[] arr)
        {
            return BitConverter.ToDouble(arr, 0);
        }
        protected override void serializeElement(double obj, Stream sink)
        {
            sink.Write(BitConverter.GetBytes(obj), 0, this.elementSize);
        }
    }
    public class DecimalFormatter : BufferedConstSizeFormatter<decimal>
    {
        public DecimalFormatter() : base(sizeof(decimal)) { }
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
    public class StringFormatter : IFormatter<string>
    {
        public StringFormatter(Encoding encoder = null)
        {
            this.encoder = encoder ?? Encoding.UTF8;
        }
        public Encoding encoder { get; }
        public string Deserialize(Stream source)
        {
            return encoder.GetString(source.ReadAll());
        }
        public void Serialize(string o, Stream sink)
        {
            var bytes = encoder.GetBytes(o);
            sink.Write(bytes,0,bytes.Length);
        }
        public int SerializeSize(string o)
        {
            return encoder.GetByteCount(o);
        }
        public bool isGreedyDeserialize => true;
    }
}
