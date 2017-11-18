using System;
using System.IO;

namespace CipherStone
{
    internal class LimitedStream : Stream
    {
        private readonly Stream _inner;
        private int _bytesLeft;
        private readonly int _allotedLength;
        public LimitedStream(Stream inner, int bytesLeft)
        {
            _inner = inner;
            this._bytesLeft = bytesLeft;
            this._allotedLength = _bytesLeft + (int)inner.Position;
        }
        public override void Flush()
        {
            _inner.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_bytesLeft == 0)
                return 0;
            if (count > _bytesLeft)
                count = _bytesLeft;
            _bytesLeft -= count;
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
                return false;
            }
        }
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }
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
                throw new NotSupportedException();
            }
        }
        public override int ReadByte()
        {
            if (_bytesLeft == 0)
                return -1;
            _bytesLeft--;
            return _inner.ReadByte();
        }
    }
}
