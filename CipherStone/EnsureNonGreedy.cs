using System.Numerics;

namespace CipherStone
{
    public static class ensureNonGreedy
    {
        public static IFormatter<T> EnsureNonGreedy<T>(this IFormatter<T> @this, bool force = false)
        {
            if (@this.isGreedyDeserialize || force)
                return new LengthFormatter<T>(@this);
            return @this;
        }
        public static IFormatter<T> EnsureNonGreedy<T>(this IFormatter<T> @this, byte terminator, byte? escape = null, bool force = false)
        {
            if (@this.isGreedyDeserialize || force)
            {
                var escapeByte = escape ?? (terminator == 0 ? (byte)1 : (byte)(terminator - 1));
                return new EscapingFormatter<T>(@this, terminator, escapeByte);
            }
            return @this;
        }
        public static IFormatter<T> EnsureNonGreedy<T>(this IFormatter<T> @this, int maxSizebytes, bool force = false)
        {
            if (!@this.isGreedyDeserialize && !force)
                return @this;
            IFormatter<BigInteger> lenFormatter = new VarSizeIntFormatter(maxSizebytes,allowNegative:false);

            return new LengthFormatter<T>(@this, lenFormatter);
        }
    }
}