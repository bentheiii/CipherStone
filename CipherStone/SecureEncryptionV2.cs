using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;

namespace CipherStone
{
    public static class SecureEncryptionV2
    {
        /*
        secure cypher scheme V2:
        1 byte- flag byte for scheme (see flags below)
        16 bits- iv of encryption
        encrypted{
            length of the plaintext in base-255, 255-terminated int
            plaintext
            the flag byte again
            padding (optional, all padding bytes are non-zero)
            zero byte
        }
        64 bytes- hash of entire message with key as salt (if hashing is enbled)

        the flag byte is presented twice so that the flag can't be easily edited.
        NOTE: an attacker can still remove the hashing with a 1/256 chance of success. This can be countered by DEMANDING the cypher have its hashing bit flag set.

        size overhead:
        no options= 20 bytes+log_255(|plaintext|)
        hashing= +64 bytes

        flag byte:
        the first six bits are ALWAYS 100001 (33)
        7th bit (64): whether hashing is enabled
        */
        private static readonly IFormatter<BigInteger> SizeFormatter = new TerminateIntegerFormatter();

        [Flags]
        public enum EncryptionOptions { Preamble = (byte)0b100001_00, Hashing = (byte)0b100001_10 }

        public static void Encrypt(Stream sink, Stream plainText, byte[] key,
            EncryptionOptions options = EncryptionOptions.Preamble, int paddingSize = 0)
        {
            if (key.Length != Encryption.KEY_LENGTH)
                key = Encryption.GenValidKey(key);

            SHA512 hashSink = null;
            if (options.HasFlag(EncryptionOptions.Hashing))
            {
                hashSink = SHA512.Create();
                hashSink.TransformBlock(key);
            }


            Stream split;
            if (hashSink != null)
                split = new SplitStream(sink, new HashAlgorithmToStream(hashSink));
            else
                split = sink;

            split.WriteByte((byte)options);

            using (var encSink = Encryption.EncryptStream(split, key, out byte[] iv))
            {
                split.Write(iv, 0, iv.Length);

                encSink.Write(plainText.Length - plainText.Position, SizeFormatter);

                plainText.CopyTo(encSink);

                encSink.WriteByte((byte)options);

                if (paddingSize > 0)
                {
                    using (RandomNumberGenerator gen = new RNGCryptoServiceProvider())
                    {
                        var pads = new byte[paddingSize];
                        gen.GetNonZeroBytes(pads);
                        encSink.Write(pads, 0, paddingSize);
                    }
                }

                encSink.WriteByte(0);
                encSink.FlushFinalBlock();
                encSink.Clear();
            }

            if (options.HasFlag(EncryptionOptions.Hashing))
            {
                hashSink.TransformFinalBlock();
                sink.Write(hashSink.Hash);
                hashSink.Dispose();
            }
        }
        public static byte[] Encrypt(byte[] plainText, byte[] key,
            EncryptionOptions options = EncryptionOptions.Preamble, int paddingSize = 0)
        {
            using (var input = new MemoryStream(plainText))
            using (var output = new MemoryStream())
            {
                Encrypt(output, input, key, options, paddingSize);
                return output.ToArray();
            }
        }
        public static int EncSize(int plainSize,
            EncryptionOptions options = EncryptionOptions.Preamble, int paddingSize = 0)
        {
            int ceilB(int s, int b = 16)
            {
                if (s % b == 0)
                    return s;
                return s + b - (s % b);
            }
            return 1 + 16 + ceilB(SizeFormatter.SerializeSize(plainSize) + plainSize + 1 + paddingSize + 1) + 
                (options.HasFlag(EncryptionOptions.Hashing) ? 64 : 0);
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

        public static void Decrypt(Stream source, Stream sink, byte[] key,
            EncryptionOptions demandOptions = EncryptionOptions.Preamble)
        {
            if (key.Length != Encryption.KEY_LENGTH)
                key = Encryption.GenValidKey(key);
            var publicOption = (EncryptionOptions)source.ReadByte();
            if (!publicOption.HasFlag(demandOptions))
                throw new InsufficientOptionException(demandOptions, publicOption);
            HashAlgorithm hashSink = null;
            Stream split = source;
            if (publicOption.HasFlag(EncryptionOptions.Hashing))
            {
                hashSink = SHA512.Create();
                hashSink.TransformBlock(key);
                hashSink.TransformBlock((byte)publicOption);

                split = new StreamSpier(split, new HashAlgorithmToStream(hashSink));
            }
            var iv = new byte[Encryption.IV_LENGTH];
            split.Read(iv, 0, iv.Length);
            using (var decStream = Encryption.DecryptStream(split, key, iv))
            {
                var size = (int)decStream.Read(SizeFormatter);
                decStream.CopyToLimited(sink, size);
                var privateOption = (EncryptionOptions)decStream.ReadByte();
                if (privateOption != publicOption)
                    throw new OptionMismatchException(publicOption, privateOption);
                while (decStream.ReadByte() != 0)
                {
                }
                decStream.Clear();
            }

            if (publicOption.HasFlag(EncryptionOptions.Hashing))
            {
                hashSink.TransformFinalBlock();
                byte[] statedHash = new byte[hashSink.HashSize/8];
                var read = source.Read(statedHash, 0, hashSink.HashSize/8);
                if (read  != hashSink.HashSize/8 || !statedHash.SequenceEqual(hashSink.Hash))
                {
                    throw new HashMismatchException(hashSink.Hash, statedHash);
                }
                hashSink.Dispose();
            }
        }
        public static byte[] Decrypt(Stream source, byte[] key,
            EncryptionOptions demandOptions = EncryptionOptions.Preamble)
        {
            using (var output = new MemoryStream())
            {
                Decrypt(source, output, key, demandOptions);
                return output.ToArray();
            }
        }
        public static byte[] Decrypt(byte[] source, byte[] key,
            EncryptionOptions demandOptions = EncryptionOptions.Preamble)
        {
            using (var output = new MemoryStream())
            using (var input = new MemoryStream(source))
            {
                Decrypt(input, output, key, demandOptions);
                return output.ToArray();
            }
        }
    }
}