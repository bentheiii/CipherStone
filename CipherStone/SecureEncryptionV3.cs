using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using WhetStone.Looping;
using WhetStone.Streams;

namespace CipherStone
{
    public static class SecureEncryptionV3
    {
        /*
        The V3 has no secure hashing or length formatting but allows to directly transition between streams.
        format:
        1 byte- option byte flag and preamble
        16 bits- iv of encryption
        encrypted{
            padding (non-zero bytes)
            zero byte
            8 bytes- first 8 bytes of hash of the key (if hashing is enabled)
            the flag byte again
            plaintext
        }

        size overhead:
        no options= 19 bytes+padding
        hashing= +8 bytes

        flag byte:
        the first six bits are ALWAYS 110001
        7th bit: whether hashing is enabled
        */
        [Flags]
        public enum EncryptionOptions { Preamble = 0b110001_00, Hashing = 0b110001_10 }
        public static CryptoStream EncryptStream(Stream sink, byte[] key, EncryptionOptions options = EncryptionOptions.Preamble, int paddingSize = 0)
        {
            if (key.Length != Encryption.KEY_LENGTH)
                key = Encryption.GenValidKey(key);
            sink.WriteByte((byte)options);
            var encSink = Encryption.EncryptStream(sink, key, out byte[] iv, PaddingMode.PKCS7);
            sink.Write(iv);

            if (paddingSize > 0)
            {
                using (RandomNumberGenerator gen = new RNGCryptoServiceProvider())
                {
                    var pads = new byte[paddingSize];
                    gen.GetNonZeroBytes(pads);
                    encSink.Write(pads);
                }
            }
            encSink.WriteByte(0);

            if (options.HasFlag(EncryptionOptions.Hashing))
            {
                encSink.Write(Sha2Hashing.Hash(key), 0, 8);
            }

            encSink.WriteByte((byte)options);

            return encSink;
        }
        public static int EncyptSize(int size, EncryptionOptions options = EncryptionOptions.Preamble, int paddingSize = 0)
        {
            int ceilB(int s, int b = 16)
            {
                return s + b - (s % b);
            }
            return 17 + ceilB(size + (options.HasFlag(EncryptionOptions.Hashing) ? 8 : 0) + 1 + paddingSize + 1);
        }
        public class InsufficientOptionException : Exception
        {
            public InsufficientOptionException(EncryptionOptions demanded, EncryptionOptions present)
            {
                this.demanded = demanded;
                this.present = present;
            }
            public EncryptionOptions demanded { get; }
            public EncryptionOptions present { get; }
            public override string ToString()
            {
                return $"Present encryption options are insufficient. Present are {present}. The function call demands at least {demanded}";
            }
        }
        public class OptionMismatchException : Exception
        {
            public OptionMismatchException(EncryptionOptions stated, EncryptionOptions encrypted)
            {
                this.stated = stated;
                this.encrypted = encrypted;
            }
            public EncryptionOptions stated { get; }
            public EncryptionOptions encrypted { get; }
            public override string ToString()
            {
                return $"Encryption option mismatch. {stated} (public) vs {encrypted} (private)";
            }
        }
        public class HashMismatchException : Exception
        {
            public HashMismatchException(byte[] calculated, byte[] expected)
            {
                this.calculated = calculated;
                this.expected = expected;
            }
            public byte[] calculated { get; }
            public byte[] expected { get; }
            public override string ToString()
            {
                return "Hash mismatch";
            }
        }
        public static CryptoStream DecryptStream(Stream source, byte[] key, EncryptionOptions demandOptions = EncryptionOptions.Preamble)
        {
            if (key.Length != Encryption.KEY_LENGTH)
                key = Encryption.GenValidKey(key);
            var publicOption = (EncryptionOptions)source.ReadByte();
            if (!publicOption.HasFlag(demandOptions))
                throw new InsufficientOptionException(demandOptions, publicOption);
            byte[] iv = source.Read(16);
            var decStream = Encryption.DecryptStream(source, key, iv, PaddingMode.PKCS7);
            while (decStream.ReadByte() != 0){}
            if (publicOption.HasFlag(EncryptionOptions.Hashing))
            {
                var statedHash = decStream.Read(8);
                var calcHash = Sha2Hashing.Hash(key).Take(8);
                if (!statedHash.SequenceEqual(calcHash))
                    throw new HashMismatchException(calcHash.ToArray(), statedHash);
            }
            var privateOption = (EncryptionOptions)decStream.ReadByte();
            if (privateOption != publicOption)
                throw new OptionMismatchException(publicOption, privateOption);
            return decStream;
        }

        public static byte[] Encrypt(byte[] plainText, byte[] key, EncryptionOptions options = EncryptionOptions.Preamble, int paddingSize = 0)
        {
            using (var ms = new MemoryStream())
            using (var s = EncryptStream(ms, key, options, paddingSize))
            {
                s.Write(plainText);
                s.FlushFinalBlock();
                return ms.ToArray();
            }
        }
        public static byte[] Decrypt(byte[] cypherText, byte[] key, EncryptionOptions demandOptions = EncryptionOptions.Preamble)
        {
            using (var ms = new MemoryStream(cypherText))
            using (var s = DecryptStream(ms, key, demandOptions))
            {
                var ret = s.ReadAll();
                s.Clear();
                return ret;
            }
        }
    }
}
