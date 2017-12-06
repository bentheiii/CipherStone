using System;
using System.IO;
using System.Linq;
using WhetStone.Looping;

namespace CipherStone
{
    public class TerminateByteFormatter<T> : IFormatter<T>
    {
        public TerminateByteFormatter(byte[] terminatorToken, IFormatter<T> inner, bool checkInput = false, bool demandTerminator = false)
        {
            this.terminatorToken = terminatorToken;
            this.inner = inner;
            this.checkInput = checkInput;
            this.demandTerminator = demandTerminator;
        }
        private const int Chunksize = 256;
        public byte[] terminatorToken { get; }
        public IFormatter<T> inner { get; }
        public bool checkInput { get; }
        public bool demandTerminator { get; }
        private T DeserializeWithSeek(Stream source)
        {
            int bufferSize = Chunksize + terminatorToken.Length;
            ResizingArray<byte> retBytes = new ResizingArray<byte>();
            while (true)
            {
                var buffer = new byte[bufferSize];

                int endTake = terminatorToken.Length;
                if (retBytes.Count < endTake)
                    endTake = retBytes.Count;
                int writeSkip = terminatorToken.Length - endTake;

                retBytes.Skip(retBytes.Count - endTake).CopyTo(buffer, writeSkip);
                int written = source.Read(buffer, terminatorToken.Length, Chunksize);
                if (written == 0)
                {
                    if (demandTerminator)
                        throw new EndOfStreamException("stream ended without terminator " + terminatorToken);
                    break;
                }
                var bufferView = Enumerable.Take(buffer, written + terminatorToken.Length);
                var searchArea = bufferView.Skip(terminatorToken.Length);
                var foundSubs = searchArea.SearchForSub(terminatorToken);
                int terminatorIndex = foundSubs.FirstOrDefault((-1, null)).startIndex;
                if (terminatorIndex == -1)
                {

                    retBytes.AddRange(bufferView.Skip(terminatorToken.Length));
                }
                else
                {
                    int readTooMuch = written - terminatorIndex - terminatorToken.Length;
                    retBytes.AddRange(Enumerable.Skip(buffer.Take(terminatorIndex + terminatorToken.Length), terminatorToken.Length));
                    source.Position -= readTooMuch;
                    break;
                }
            }
            return inner.deserialize(retBytes.arr);
        }
        private T DeserializeWithoutSeek(Stream source)
        {
            ResizingArray<byte> retBytes = new ResizingArray<byte>();
            int chop = terminatorToken.Length;
            while (retBytes.Count < terminatorToken.Length ||
                !retBytes.Skip(retBytes.Count - terminatorToken.Length).SequenceEqual(terminatorToken))
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
            return inner.deserialize(retBytes.arr, 0, retBytes.arr.Length - chop);
        }
        public T Deserialize(Stream source)
        {
            if (source.CanSeek)
                return DeserializeWithSeek(source);
            return DeserializeWithoutSeek(source);
        }
        public void Serialize(T o, Stream sink)
        {
            var bytes = inner.serialize(o);

            if (checkInput && bytes.SearchForSub(terminatorToken).Any())
                throw new ArgumentException("string cannot be serialized because it contains the terminator " + terminatorToken.StrConcat());
            
            sink.Write(bytes, 0, bytes.Length);
            sink.Write(terminatorToken, 0, terminatorToken.Length);
        }
        public int SerializeSize(T o)
        {
            int os = inner.SerializeSize(o);
            if (os < 0)
                return -1;
            return os + terminatorToken.Length;
        }
        public bool isGreedyDeserialize => false;
    }
    public static class TerminateStringFormatter
    {
        public static TerminateByteFormatter<string> create(string terminatorToken, IFormatter<string> inner = null, bool checkInput = false, bool demandTerminator = false)
        {
            inner = inner ?? new StringFormatter();
            var token = inner.serialize(terminatorToken);

            return new TerminateByteFormatter<string>(token, inner, checkInput, demandTerminator);
        }

    }
}
