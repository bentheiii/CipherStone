using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WhetStone.Looping;

namespace CipherStone
{
    internal class SplitStream : Stream
    {
        private readonly Stream[] _inners;
        public SplitStream(params Stream[] inners)
        {
            _inners = inners;
        }
        public override void Flush()
        {
            _inners.Do(a=>a.Flush());
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
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            _inners.Do(a=>a.Write(buffer, offset, count));
        }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => _inners.All(a => a.CanWrite);
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override void WriteByte(byte value)
        {
            _inners.Do(a=>a.WriteByte(value));
        }
    }
    internal class StreamSpier : Stream
    {
        private readonly Stream _inner;
        private readonly Stream _spy;
        public StreamSpier(Stream inner, Stream spy)
        {
            _inner = inner;
            _spy = spy;
        }
        public override void Flush()
        {
            _inner.Flush();
            _spy.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var ret = _inner.Read(buffer, offset, count);
            _spy.Write(buffer, offset, ret);
            return ret;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
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
                return _inner.Length;
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
                _inner.Position = value;
            }
        }
    }
    internal class HashAlgorithmToStream : Stream
    {
        private readonly HashAlgorithm _inner;
        public HashAlgorithmToStream(HashAlgorithm inner)
        {
            _inner = inner;
        }
        public override void Flush()
        {
            _inner.TransformFinalBlock();
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
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.TransformBlock(buffer, offset, count);
        }
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public override void WriteByte(byte value)
        {
            _inner.TransformBlock(value);
        }
    }
}
