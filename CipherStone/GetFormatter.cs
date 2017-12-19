using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using WhetStone.Looping;

namespace CipherStone
{
    public static class getFormatter
    {
        private static IEnumerable<ConstructorInfo> getConstructors(this Type @this, Type[] paramTypes)
        {
            return @this.GetConstructors().Where(a =>
                a.GetParameters().Length >= paramTypes.Length &&
                a.GetParameters().Zip(paramTypes).All(x => x.Item1.ParameterType.IsAssignableFrom(x.Item2)) &&
                a.GetParameters().Skip(paramTypes.Length).All(x => x.HasDefaultValue)
            ).OrderBy(a => a.GetParameters().Length);
        }
        public enum DefaultFormatter { None, DotNet,
        #if JSON
            Json
        #endif
        }
        private static readonly Lazy<MethodInfo> Mi = new Lazy<MethodInfo>(() =>
                typeof(getFormatter).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(a=>
                {
                    return (a.Name == nameof(GetFormatter)) &&
                           a.GetParameters().Select(x => x.ParameterType)
                           .SequenceEqual(new[] {typeof(DefaultFormatter)});
                })
            , LazyThreadSafetyMode.None);
        public static object GetFormatter(Type t, DefaultFormatter onDefault = DefaultFormatter.DotNet)
        {
            var meth = Mi.Value.MakeGenericMethod(t);
            return meth.Invoke(null, new object[] { onDefault });
        }
        public static IFormatter<T> GetFormatter<T>(DefaultFormatter onDefault = DefaultFormatter.DotNet)
        {
            IFormatter<T> ConType<NONGEN>(Type[] genArgs, object[] conArgs)
            {
                var retType = typeof(NONGEN).GetGenericTypeDefinition().MakeGenericType(genArgs);
                var con = retType.getConstructors(conArgs.Select(a => a.GetType()).ToArray()).FirstOrDefault();
                if (con == null)
                    throw new Exception("constructor not found");
                if (con.GetParameters().Length > conArgs.Length)
                    conArgs = conArgs.Concat(Type.Missing.Enumerate(con.GetParameters().Length - conArgs.Length)).ToArray();
                return (IFormatter<T>)con.Invoke(conArgs);
            }

            var tT = typeof(T);

            if (tT == typeof(byte))
            {
                return new ByteFormatter().cast<T>();
            }
            if (tT == typeof(sbyte))
            {
                return new SByteFormatter().cast<T>();
            }
            if (tT == typeof(short))
            {
                return new ShortFormatter().cast<T>();
            }
            if (tT == typeof(ushort))
            {
                return new UShortFormatter().cast<T>();
            }
            if (tT == typeof(int))
            {
                return new IntFormatter().cast<T>();
            }
            if (tT == typeof(uint))
            {
                return new UIntFormatter().cast<T>();
            }
            if (tT == typeof(long))
            {
                return new LongFormatter().cast<T>();
            }
            if (tT == typeof(long))
            {
                return new ULongFormatter().cast<T>();
            }
            if (tT == typeof(float))
            {
                return new FloatFormatter().cast<T>();
            }
            if (tT == typeof(double))
            {
                return new DoubleFormatter().cast<T>();
            }
            if (tT == typeof(decimal))
            {
                return new DecimalFormatter().cast<T>();
            }
            if (tT == typeof(string))
            {
                return new StringFormatter().cast<T>();
            }
            if (tT == typeof(char))
            {
                return (IFormatter<T>)new CharFormatter();
            }
            if (tT == typeof(BigInteger))
            {
                return new BigIntFormatter().cast<T>(); 
            }
            if (tT.IsGenericType)
            {
                var gd = tT.GetGenericTypeDefinition();
                var ga = tT.GetGenericArguments();
                var inners = ga.Select(a=>GetFormatter(a,onDefault)).ToArray();

                if (gd == typeof((object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object,object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object, object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object, object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object, object, object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object, object, object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object, object, object, object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object, object, object, object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object, object, object, object, object, object>>(ga, inners);
                }
                if (gd == typeof((object, object, object, object, object, object, object, object)).GetGenericTypeDefinition())
                {
                    return ConType<TupleFormatter<object, object, object, object, object, object, object, object>>(ga, inners);
                }
                if (gd == typeof(Dictionary<object, object>).GetGenericTypeDefinition())
                {
                    return ConType<DictionaryFormatter<object, object>>(ga, inners);
                }
            }
            if (tT.IsArray)
            {
                var innerType = tT.GetElementType();
                var inner = GetFormatter(innerType, onDefault);
                return ConType<ArrayFormatter<object>>(new[] {innerType}, new[] {inner});
            }
            switch (onDefault)
            {
                case DefaultFormatter.DotNet:
                    return new DotNetFormatter<T>();
#if JSON
                case DefaultFormatter.Json:
                    return new JsonFormatter<T>();
#endif
                default:
                    throw new ArgumentException("fallback on none default serializer");
            }
        }
    }
}