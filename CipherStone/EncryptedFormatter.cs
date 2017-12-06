using System.IO;
using WhetStone.Random;
using WhetStone.Streams;

namespace CipherStone
{
    public class EncryptedFormatterV1<T> : IFormatter<T>
    {
        private readonly IFormatter<T> _inner;
        private readonly byte[] _key;
        private readonly (int minInc,int maxExc) _paddingRange;
        public EncryptedFormatterV1(IFormatter<T> inner, byte[] key, (int, int)? paddingRange = null )
        {
            _inner = inner;
            _key = key;
            _paddingRange = paddingRange ?? (0,1);
        }
        private int _getPadding()
        {
            return new GlobalRandomGenerator().Int(_paddingRange.minInc, _paddingRange.maxExc);
        }
        public T Deserialize(Stream source)
        {
            return _inner.deserialize(SecureEncryptionV1.Decrypt(source.ReadAll(), _key));
        }
        public void Serialize(T o, Stream sink)
        {
            var padding = _getPadding();
            var arr = SecureEncryptionV1.Encrypt(_inner.serialize(o), _key, padding);
            sink.Write(arr,0,arr.Length);
        }
        public int SerializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => true;
    }
    public class EncryptedFormatterV2<T> : IFormatter<T>
    {
        private readonly IFormatter<T> _inner;
        private readonly byte[] _key;
        private readonly (int minInc, int maxExc) _paddingRange;
        private readonly SecureEncryptionV2.EncryptionOptions _options;
        public EncryptedFormatterV2(IFormatter<T> inner, byte[] key, SecureEncryptionV2.EncryptionOptions options, (int, int)? paddingRange = null)
        {
            _inner = inner;
            _key = key;
            _options = options;
            _paddingRange = paddingRange ?? (0, 1);
        }
        private int _getPadding()
        {
            return new GlobalRandomGenerator().Int(_paddingRange.minInc, _paddingRange.maxExc);
        }
        public T Deserialize(Stream source)
        {
            return _inner.deserialize(SecureEncryptionV2.Decrypt(source, _key, _options));
        }
        public void Serialize(T o, Stream sink)
        {
            var padding = _getPadding();
            var arr = SecureEncryptionV2.Encrypt(_inner.serialize(o), _key, _options, padding);
            sink.Write(arr, 0, arr.Length);
        }
        public int SerializeSize(T o)
        {
            if (_paddingRange.maxExc - _paddingRange.minInc > 1)
                return -1;
            var i = _inner.SerializeSize(o);
            if (i == -1)
                return -1;
            return SecureEncryptionV2.EncSize(i,_options,_paddingRange.minInc);
        }
        public bool isGreedyDeserialize => false;
    }
    public class EncryptedFormatterV3<T> : IFormatter<T>
    {
        private readonly IFormatter<T> _inner;
        private readonly byte[] _key;
        private readonly SecureEncryptionV3.EncryptionOptions _options;
        public EncryptedFormatterV3(IFormatter<T> inner, byte[] key, SecureEncryptionV3.EncryptionOptions options)
        {
            _inner = inner;
            _key = key;
            _options = options;
        }
        public T Deserialize(Stream source)
        {
            T ret;
            using (var s = SecureEncryptionV3.DecryptStream(source, _key, _options))
            {
                ret = _inner.Deserialize(s);
                s.Clear();
            }
            return ret;
        }
        public void Serialize(T o, Stream sink)
        {
            using (var s = SecureEncryptionV3.EncryptStream(sink, _key, _options))
            {
                _inner.Serialize(o,s);
                s.FlushFinalBlock();
            }
        }
        public int SerializeSize(T o)
        {
            var i = _inner.SerializeSize(o);
            if (i == -1)
                return -1;
            return SecureEncryptionV3.EncyptSize(i, _options);
        }
        public bool isGreedyDeserialize => _inner.isGreedyDeserialize;
    }
}