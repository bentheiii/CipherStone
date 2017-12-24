using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhetStone.Looping;

namespace CipherStone
{
    internal class EscapedStream : Stream
    {
        private readonly Stream _inner;
        private readonly byte _toEscape;
        private readonly byte _escape;
        private bool _ended = false;
        public EscapedStream(Stream inner, byte toEscape, byte escape)
        {
            _inner = inner;
            _toEscape = toEscape;
            _escape = escape;
        }
        public override int ReadByte()
        {
            int r;
            if (_ended || (r = _inner.ReadByte()) == -1)
            {
                _ended = true;
                return -1;
            }
            if (r == _escape)
            {
                r = _inner.ReadByte();
                if (r == -1)
                    throw new EndOfStreamException("lone escape byte");
                if (r != _escape && r != _toEscape)
                    throw new Exception("escaped byte is invalid");
                return r;
            }
            if (r == _toEscape)
            {
                _ended = true;
                return -1;
            }
            return r;
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
            int ret = 0;
            if (!_inner.CanSeek)
            {
                foreach (var c in range.Range(count))
                {
                    int r = ReadByte();
                    if (r == -1)
                        break;
                    buffer[c + offset] = (byte)r;
                    ret++;
                }
            }
            else
            {
                int extra = 0;
                while (count > 0)
                {
                    //todo check if there is room for count+extra
                    var read = _inner.Read(buffer, offset, count + extra);
                    extra = 0;
                    if (read == 0)
                        break;
                    var breaks = new List<(bool end, int index)>();
                    int trim = 0;
                    for (int i = offset; i < read+offset; i++)
                    {
                        if (buffer[i] == _toEscape)
                        {
                            breaks.Add((true,i));
                            break;
                        }
                        if (buffer[i] == _escape)
                        {
                            if (i == read + offset - 1)
                            {
                                trim++;
                                read--;
                                _inner.Position--;
                                break;
                            }
                            else
                            {
                                breaks.Add((false,i));
                                i++;
                            }
                        }
                    }
                    breaks.Add((false, read+offset));

                    var removed = 0;
                    ret += breaks.First().index - offset;
                    var termInd = -1;
                    foreach (var segment in breaks.Trail(2))
                    {
                        var ((end, startInd), (_, endInd)) = segment;
                        startInd -= removed;
                        endInd -= removed;
                        removed++;
                        if (end)
                        {
                            //ret--;
                            termInd = startInd;
                            break;
                        }
                        Array.Copy(buffer,startInd+removed,buffer,startInd,endInd-startInd-1);
                        ret += endInd - startInd - 1;
                    }

                    if (termInd >= 0)
                    {
                        var overdrawn = read + offset - termInd - removed;
                        _inner.Position -= overdrawn;
                        break;
                    }

                    offset = (offset + read - removed);
                    count = removed+trim;
                    if (trim > 0 && count == 1)
                        extra++;
                }
            }
            return ret;
        }
        public override void WriteByte(byte value)
        {
            if (value == _escape || value == _toEscape)
            {
                _inner.WriteByte(_escape);
            }
            _inner.WriteByte(value);
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
                return _inner.CanWrite;
            }
        }
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
                throw new NotSupportedException();
            }
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            var bufferView = buffer.Slice(offset, length: count);
            var toEscape = bufferView.CountBind().Where(a => a.element == _escape || a.element == _toEscape).ToArray();
            if (!toEscape.Any())
            {
                _inner.Write(buffer, offset, count);
                return;
            }

            var (element, index) = toEscape.First();
            _inner.Write(buffer, offset, index);
            //WriteByte(element);

            var slices = toEscape.Concat(((byte)0, buffer.Length - offset).Enumerate()).Trail(2);
            foreach (var slice in slices)
            {
                var ((startEl, startInd), (_,endInd)) = slice;
                WriteByte(startEl);
                startInd++;
                _inner.Write(buffer, offset + startInd, endInd-startInd);
            }
        }
        public void writeToEscape()
        {
            _inner.WriteByte(_toEscape);
        }
        public void readToEscape()
        {
            while (!_ended)
            {
                ReadByte();
            }
        }
    }
    public class EscapingFormatter<T> : IFormatter<T>
    {
        public IFormatter<T> inner { get; }
        public byte TerminatorByte { get; }
        public byte EscapeByte { get; }
        public EscapingFormatter(IFormatter<T> inner, byte terminatorByte = 127, byte escapeByte = 126)
        {
            if (!inner.isGreedyDeserialize)
                throw new NotImplementedException("non-greedy inners are not yet implemented for escaping formatters");
            this.inner = inner;
            TerminatorByte = terminatorByte;
            EscapeByte = escapeByte;
        }
        private EscapedStream wrap(Stream s)
        {
            return new EscapedStream(s, TerminatorByte, EscapeByte);
        }
        public bool isGreedyDeserialize => false;
        public T Deserialize(Stream source)
        {
            using (var w = wrap(source))
            {
                var ret = inner.Deserialize(w);
                w.readToEscape();
                return ret;
            }
        }
        public void Serialize(T o, Stream sink)
        {
            using (var w = wrap(sink))
            {
                inner.Serialize(o, w);
                w.writeToEscape();
            }
        }
        public int SerializeSize(T o)
        {
            return -1;
        }
    }
}
