using System;
using System.IO;
using System.Linq;
using System.Text;
using WhetStone.Looping;

namespace CipherStone
{
    /*
    public class TerminateStringFormatter : TerminateByteFormatter<string>
    {
        //inner must be 1-to-1
        public TerminateStringFormatter(string terminatorToken, bool demandTerminator = false, bool checkInput = false, IFormatter<string> inner = null)
        {
            this.terminatorToken = terminatorToken;
            this.demandTerminator = demandTerminator;
            this.checkInput = checkInput;
            this.inner = inner ?? new StringFormatter(Encoding.UTF8);
            this._needle = this.inner.serialize(this.terminatorToken);
        }
        private readonly byte[] _needle;
        private const int Chunksize = 1024;
        public string terminatorToken { get; }
        public IFormatter<string> inner { get; }
        public bool checkInput { get; }
        public bool demandTerminator { get; }
        private string DeserializeWithSeek(Stream source)
        {
            int bufferSize = Chunksize + _needle.Length;
            ResizingArray<byte> retBytes = new ResizingArray<byte>();
            while (true)
            {
                var buffer = new byte[bufferSize];

                int endTake = _needle.Length;
                if (retBytes.Count < endTake)
                    endTake = retBytes.Count;
                int writeSkip = _needle.Length - endTake;

                retBytes.Skip(retBytes.Count - endTake).CopyTo(buffer, writeSkip);
                int written = source.Read(buffer, _needle.Length, Chunksize);
                if (written == 0)
                {
                    if (demandTerminator)
                        throw new EndOfStreamException("stream ended without terminator " + terminatorToken);
                    break;
                }
                var bufferView = buffer.Take(written + _needle.Length);
                var searchArea = bufferView.Skip(_needle.Length);
                var foundSubs = searchArea.SearchForSub(_needle);
                int terminatorIndex = foundSubs.FirstOrDefault((-1, null)).startIndex;
                if (terminatorIndex == -1)
                {

                    retBytes.AddRange(bufferView.Skip(_needle.Length));
                }
                else
                {
                    int readTooMuch = written - terminatorIndex - _needle.Length;
                    retBytes.AddRange(buffer.Take(terminatorIndex + _needle.Length).Skip(_needle.Length));
                    source.Position -= readTooMuch;
                    break;
                }
            }
            return inner.deserialize(retBytes.arr);
        }
        private string DeserializeWithoutSeek(Stream source)
        {
            ResizingArray<byte> retBytes = new ResizingArray<byte>();
            int chop = _needle.Length;
            while (retBytes.Count < _needle.Length ||
                !retBytes.Skip(retBytes.Count - _needle.Length).SequenceEqual(_needle))
            {
                int b = source.ReadByte();
                if (b == -1)
                {
                    if (demandTerminator)
                        throw new EndOfStreamException("stream ended without terminator " + terminatorToken);
                    chop = 0;
                    break;
                }
                retBytes.Add((byte)b);
            }
            return inner.deserialize(retBytes.arr,0,retBytes.arr.Length - chop);
        }
        public string Deserialize(Stream source)
        {
            if (source.CanSeek)
                return DeserializeWithSeek(source);
            return DeserializeWithoutSeek(source);
        }
        public void Serialize(string o, Stream sink)
        {
            if (checkInput && o.IndexOf(terminatorToken) >= 0)
                throw new ArgumentException("string cannot be serialized because it contains the terminator "+terminatorToken);

            var bytes = inner.serialize(o);
            var termBytes = inner.serialize(terminatorToken);
            sink.Write(bytes, 0, bytes.Length);
            sink.Write(termBytes, 0, termBytes.Length);
        }
        public int SerializeSize(string o)
        {
            int os = inner.SerializeSize(o), ts = inner.SerializeSize(terminatorToken);
            if (os < 0 || ts < 0)
                return -1;
            return os + ts;
        }
        public bool isGreedyDeserialize => false;
    }
    */
}
