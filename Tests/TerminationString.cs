using System;
using System.IO;
using System.Linq;
using CipherStone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    public class unseekingStream : Stream
    {
        private readonly Stream _inner;
        public unseekingStream(Stream inner)
        {
            _inner = inner;
        }
        public override void Flush()
        {
            _inner.Flush();
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new Exception();
        }
        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
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
                throw new Exception();
            }
        }
    }
    [TestClass]
    public class TerminationString
    {
        [TestMethod] public void Simple_TermString_singleChar()
        {
            var formatter = TerminateStringFormatter.create("-");

            void check1(string data)
            {
                var bytes = formatter.serialize(data);
                var dec = formatter.deserialize(bytes);
                Assert.AreEqual(dec, data);
            }
            void checkMany(params string[] data)
            {
                var mem = new MemoryStream();
                foreach (var s in data)
                {
                    mem.Write(s,formatter);
                }
                mem.Seek(0, SeekOrigin.Begin);
                foreach (var s in data)
                {
                    var dec = mem.Read(formatter);
                    Assert.AreEqual(s,dec);
                }
            }

            check1("a");
            check1("abra kadabra");
            check1(new string('a',300));
            checkMany("a","abrakadabra", new string('a', 300), "tooMany", "toofar");
        }
        [TestMethod] public void Simple_TermString_multichar()
        {
            var formatter = TerminateStringFormatter.create("<EOF>");

            void check1(string data)
            {
                var bytes = formatter.serialize(data);
                var dec = formatter.deserialize(bytes);
                Assert.AreEqual(dec, data);
            }
            void checkMany(params string[] data)
            {
                var mem = new MemoryStream();
                foreach (var s in data)
                {
                    mem.Write(s, formatter);
                }
                mem.Seek(0, SeekOrigin.Begin);
                foreach (var s in data)
                {
                    var dec = mem.Read(formatter);
                    Assert.AreEqual(s, dec);
                }
            }

            check1("a");
            check1("abra kadabra");
            check1(new string('a', 300));
            checkMany("a", "abrakadabra", new string('a', 300), "tooMany", "toofar");
        }
        [TestMethod] public void Simple_TermString_multichar_tailcut()
        {
            var formatter = TerminateStringFormatter.create("<EOF>");
            var tailcutter = formatter.inner;
            void check1(string data)
            {
                var bytes = tailcutter.serialize(data);
                var dec = formatter.deserialize(bytes);
                Assert.AreEqual(dec, data);
            }
            void checkMany(params string[] data)
            {
                var mem = new MemoryStream();
                foreach (var s in data.Take(data.Length-1))
                {
                    mem.Write(s, formatter);
                }
                mem.Write(data.Last(), tailcutter);
                mem.Seek(0, SeekOrigin.Begin);
                foreach (var s in data)
                {
                    var dec = mem.Read(formatter);
                    Assert.AreEqual(s, dec);
                }
            }

            check1("a");
            check1("abra kadabra");
            check1(new string('a', 300));
            checkMany("a", "abrakadabra", new string('a', 300), "tooMany", "toofar");
        }

        [TestMethod] public void Simple_TermString_singleChar_unseek()
        {
            var formatter = TerminateStringFormatter.create("-");

            void check1(string data)
            {
                var ms = new MemoryStream();
                formatter.Serialize(data, ms);
                ms.Seek(0, SeekOrigin.Begin);
                var dec = formatter.Deserialize(new unseekingStream(ms));
                Assert.AreEqual(dec, data);
            }
            void checkMany(params string[] data)
            {
                Stream mem = new MemoryStream();
                foreach (var s in data)
                {
                    mem.Write(s, formatter);
                }
                mem.Seek(0, SeekOrigin.Begin);
                mem = new unseekingStream(mem);
                foreach (var s in data)
                {
                    var dec = mem.Read(formatter);
                    Assert.AreEqual(s, dec);
                }
            }

            check1("a");
            check1("abra kadabra");
            check1(new string('a', 300));
            checkMany("a", "abrakadabra", new string('a', 300), "tooMany", "toofar");
        }
        [TestMethod] public void Simple_TermString_multichar_unseek()
        {
            var formatter = TerminateStringFormatter.create("<EOF>");

            void check1(string data)
            {
                var ms = new MemoryStream();
                formatter.Serialize(data, ms);
                ms.Seek(0, SeekOrigin.Begin);
                var dec = formatter.Deserialize(new unseekingStream(ms));
                Assert.AreEqual(dec, data);
            }
            void checkMany(params string[] data)
            {
                Stream mem = new MemoryStream();
                foreach (var s in data)
                {
                    mem.Write(s, formatter);
                }
                mem.Seek(0, SeekOrigin.Begin);
                mem = new unseekingStream(mem);
                foreach (var s in data)
                {
                    var dec = mem.Read(formatter);
                    Assert.AreEqual(s, dec);
                }
            }

            check1("a");
            check1("abra kadabra");
            check1(new string('a', 300));
            checkMany("a", "abrakadabra", new string('a', 300), "tooMany", "toofar");
        }
        [TestMethod] public void Simple_TermString_multichar_tailcut_unseek()
        {
            var formatter = TerminateStringFormatter.create("<EOF>");
            var tailcutter = formatter.inner;
            void check1(string data)
            {
                var ms = new MemoryStream();
                tailcutter.Serialize(data, ms);
                ms.Seek(0, SeekOrigin.Begin);
                var dec = formatter.Deserialize(new unseekingStream(ms));
                Assert.AreEqual(dec, data);
            }
            void checkMany(params string[] data)
            {
                Stream mem = new MemoryStream();
                foreach (var s in data.Take(data.Length - 1))
                {
                    mem.Write(s, formatter);
                }
                mem.Write(data.Last(), tailcutter);
                mem.Seek(0, SeekOrigin.Begin);
                mem = new unseekingStream(mem);
                foreach (var s in data)
                {
                    var dec = mem.Read(formatter);
                    Assert.AreEqual(s, dec);
                }
            }

            check1("a");
            check1("abra kadabra");
            check1(new string('a', 300));
            checkMany("a", "abrakadabra", new string('a', 300), "tooMany", "toofar");
        }
    }
}
