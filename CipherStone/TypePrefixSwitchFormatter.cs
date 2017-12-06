using System;
using System.Collections.Generic;
using System.Numerics;

namespace CipherStone
{
    public class TypePrefixSwitchCase<T,I,C> : SelectPrefixSwitchFormatterCase<T,I,C> where I : T
    {
        public TypePrefixSwitchCase(C code, IFormatter<I> innerFormatter) : base(code, innerFormatter){}
        public override bool canSerialize(T o)
        {
            return o is I;
        }
        public override T toOuter(I inner)
        {
            return inner;
        }
        public override I toInner(T outer)
        {
            return (I)outer;
        }
    }
    public static class TypePrefixSwitchFormatter
    {
        public static IFormatter<T> getTypePrefixSwitchFormatter<T, C>(
            IEnumerable<(Type inheritedType, IGenericFormatter innerFormatter, C code)> cases,
            IFormatter<C> codeFormatter = null, IEqualityComparer<C> codeComparer = null,
            getFormatter.DefaultFormatter onDefault = getFormatter.DefaultFormatter.DotNet)
        {
            codeFormatter = codeFormatter ?? getFormatter.GetFormatter<C>();
            codeComparer = codeComparer ?? EqualityComparer<C>.Default;

            var typeprefixswitchcaseDef = typeof(TypePrefixSwitchCase<object, object, object>).GetGenericTypeDefinition();
            var formatterDef = typeof(IFormatter<object>).GetGenericTypeDefinition();
            var baseType = typeof(T);
            var codeType = typeof(C);

            IPrefixSwitchFormatterCase<T, C> getCase(Type inheritedType, IGenericFormatter innerFormatter, C code)
            {
                if (!baseType.IsAssignableFrom(inheritedType))
                    throw new ArgumentException($"Type {inheritedType} does not inherit {baseType}");
                innerFormatter = innerFormatter ?? (IGenericFormatter)getFormatter.GetFormatter(inheritedType, onDefault);
                var retType = typeprefixswitchcaseDef.MakeGenericType(baseType, inheritedType, codeType);
                var innerFormatterType = formatterDef.MakeGenericType(inheritedType);
                var con = retType.GetConstructor(new [] {codeType, innerFormatterType});
                return (IPrefixSwitchFormatterCase<T, C>)con.Invoke(new object[] {code, innerFormatter});
            }

            var formatters = new List<IPrefixSwitchFormatterCase<T, C>>();

            foreach (var (i, f, c) in cases)
            {
                formatters.Add(getCase(i,f,c));
            }

            return new PrefixSwitchFormatter<T,C>(formatters, codeFormatter, codeComparer);
        }
        
        public static IFormatter<T> getTypePrefixSwitchFormatter<T>(
            IEnumerable<(Type inheritedType, IGenericFormatter innerFormatter)> cases,
            getFormatter.DefaultFormatter onDefault = getFormatter.DefaultFormatter.DotNet)
        {
            IEnumerable<(Type, IGenericFormatter, BigInteger code)> toCases(
                IEnumerable<(Type inheritedType, IGenericFormatter innerFormatter)> primitiveCases)
            {
                var code = BigInteger.Zero;
                foreach (var (t, f) in primitiveCases)
                {
                    if (t != null)
                    {
                        yield return (t, f, code);
                    }
                    code++;
                }
            }
            return getTypePrefixSwitchFormatter<T, BigInteger>(
                toCases(cases), new TerminateIntegerFormatter(), null, onDefault
            );
        }

        public static IFormatter<T> getTypePrefixSwitchFormatter<T>(
            IEnumerable<(Type inheritedType, IGenericFormatter innerFormatter, string code)> cases,
            string terminator = " ",
            getFormatter.DefaultFormatter onDefault = getFormatter.DefaultFormatter.DotNet)
        {
            IEnumerable<(Type, IGenericFormatter, string code)> toCases(
                IEnumerable<(Type inheritedType, IGenericFormatter innerFormatter, string)> primitiveCases)
            {
                foreach (var (t, f, c) in primitiveCases)
                {
                    var name = c ?? t.Name;
                    yield return (t, f, name);
                }
            }

            return getTypePrefixSwitchFormatter<T, string>(
                toCases(cases), TerminateStringFormatter.create(terminator, demandTerminator:true), onDefault: onDefault
            );
        }
    }
}
