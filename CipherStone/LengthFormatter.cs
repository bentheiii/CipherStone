using System;
using System.IO;
using System.Numerics;

namespace CipherStone
{
    internal class LimitedStream : Stream
    {
        protected readonly Stream _inner;
        protected readonly int _allotedLength;
        private bool _expandCalled = false;
        public LimitedStream(Stream inner, int bytesLeft)
        {
            _inner = inner;
            this._allotedLength = bytesLeft + (int)inner.Position;
        }
        private int bytesLeft => (int)(_allotedLength - Position);
        public override void Flush()
        {
            _inner.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.End)
            {
                offset = Length;
                origin = SeekOrigin.Begin;
            }
            return _inner.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (bytesLeft == 0)
                return 0;
            if (count > bytesLeft)
                count = bytesLeft;
            return _inner.Read(buffer, offset, count);
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        public override bool CanRead
        {
            get
            {
                return _inner.CanRead;
            }
        }
        public override bool CanSeek
        {
            get
            {
                return _inner.CanSeek;
            }
        }
        public override bool CanWrite => false;
        public override long Length
        {
            get
            {
                return Math.Min(_inner.Length, _allotedLength);
            }
        }
        public override long Position
        {
            get
            {
                return _inner.Position;
            }
            set
            {
                Seek(value, SeekOrigin.Current);
            }
        }
        public override int ReadByte()
        {
            if (bytesLeft == 0)
                return -1;
            var ret = _inner.ReadByte();
            CheckForExpanded();
            return ret;
        }
        private void CheckForExpanded()
        {
            if (bytesLeft == 0 && _expandCalled)
            {
                onExpended();
                _expandCalled = true;
            }
        }
        protected virtual void onExpended()
        { }
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
