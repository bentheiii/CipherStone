using System;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using WhetStone.Looping;

namespace CipherStone
{
    public static class getSerializer
    {
        public enum DefaultSerializer { None, DotNet,
        #if JSON
            Json
        #endif
        }
        private static readonly Lazy<MethodInfo> Mi = new Lazy<MethodInfo>(() =>
                typeof(getSerializer).GetMethod(nameof(GetSerializer), BindingFlags.Static | BindingFlags.Public)
            , LazyThreadSafetyMode.None);
        public static IByteSerializer<T> GetSerializer<T>(DefaultSerializer onDefault = DefaultSerializer.DotNet)
        {
            object GetSerializer(Type t)
            {
                var meth = Mi.Value.MakeGenericMethod(t);
                return meth.Invoke(null, new object[] { onDefault });
            }
            IByteSerializer<T> ConType<NONGEN>(Type[] genArgs, object[] conArgs)
            {
                var retType = typeof(NONGEN).GetGenericTypeDefinition().MakeGenericType(genArgs);
                var con = retType.GetConstructor(conArgs.Select(a => a.GetType()).ToArray());
                if (con == null)
                    throw new Exception("constructor not found");
                return (IByteSerializer<T>)con.Invoke(conArgs);
            }

            var tT = typeof(T);
            if (tT == typeof(byte))
            {
                return new ByteSerializer().cast<T>();
            }
            if (tT == typeof(short))
            {
                return new ShortSerializer().cast<T>();
            }
            if (tT == typeof(int))
            {
                return new IntSerializer().cast<T>();
            }
            if (tT == typeof(long))
            {
                return new LongSerializer().cast<T>();
            }
            if (tT == typeof(long))
            {
                return new ULongSerializer().cast<T>();
            }
            if (tT == typeof(float))
            {
                return new FloatSerializer().cast<T>();
            }
            if (tT == typeof(double))
            {
                return new DoubleSerializer().cast<T>();
            }
            if (tT == typeof(decimal))
            {
                return new DecimalSerializer().cast<T>();
            }
            if (tT == typeof(string))
            {
                return new StringSerializer().cast<T>();
            }
            if (tT == typeof(BigInteger))
            {
                return new BigIntSerializer().cast<T>(); 
            }
            if (tT.IsGenericType)
            {
                var gd = tT.GetGenericTypeDefinition();
                var ga = tT.GetGenericArguments();
                var inners = ga.Select(GetSerializer).ToArray();

                if (gd == typeof((object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleSerializer<object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleSerializer<object, object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object,object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleSerializer<object, object, object, object>>(ga, inners);
                }
            }
            if (tT.IsArray)
            {
                var innerType = tT.GetElementType();
                var inner = GetSerializer(innerType);
                return ConType<ArraySerializer<object>>(new[] {innerType}, new[] {inner});
            }
            switch (onDefault)
            {
                case DefaultSerializer.DotNet:
                    return new DotNetSerializer<T>();
#if JSON
                case DefaultSerializer.Json:
                    return new JsonSerializer<T>();
#endif
                default:
                    throw new ArgumentException("fallback on none default serializer");
            }
        }
    }
}