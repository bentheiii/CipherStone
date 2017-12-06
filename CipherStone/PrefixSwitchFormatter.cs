using System;
using System.Collections.Generic;
using System.IO;

namespace CipherStone
{
    public interface IPrefixSwitchFormatterCase<T, out C>: IFormatter<T>
    {
        C code { get; }
        bool canSerialize(T o);
    }
    public abstract class SelectPrefixSwitchFormatterCase<T, I, C> : IPrefixSwitchFormatterCase<T,C>
    {
        protected SelectPrefixSwitchFormatterCase(C code, IFormatter<I> innerFormatter)
        {
            this.code = code;
            this.innerFormatter = innerFormatter;
        }
        public C code { get; }
        public IFormatter<I> innerFormatter { get; }
        public abstract bool canSerialize(T o);
        public abstract T toOuter(I inner);
        public abstract I toInner(T outer);
        public T Deserialize(Stream source)
        {
            return toOuter(source.Read(innerFormatter));
        }
        public void Serialize(T o, Stream sink)
        {
            innerFormatter.Serialize(toInner(o), sink);
        }
        public int SerializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize => innerFormatter.isGreedyDeserialize;
    }
    public class PrefixSwitchFormatter<T, C> : IFormatter<T>
    {
        private readonly IDictionary<C, IPrefixSwitchFormatterCase<T, C>> _cases;
        public IFormatter<C> codeFormatter { get; }
        public PrefixSwitchFormatter(IEnumerable<IPrefixSwitchFormatterCase<T,C>> cases,IFormatter<C> codeFormatter = null, IEqualityComparer<C> codeComparer = null)
        {
            codeFormatter = codeFormatter ?? getFormatter.GetFormatter<C>();
            this.codeFormatter = codeFormatter.EnsureNonGreedy();
            codeComparer = codeComparer ?? EqualityComparer<C>.Default;
            _cases = new Dictionary<C, IPrefixSwitchFormatterCase<T, C>>(codeComparer);
            foreach (var @case in cases)
            {
                if (@case.isGreedyDeserialize)
                    isGreedyDeserialize = true;
                var code = @case.code;
                if (_cases.ContainsKey(code))
                    throw new Exception("two cases share code: "+code);
                _cases[code] = @case;
            }
        }
        public T Deserialize(Stream source)
        {
            var code = codeFormatter.Deserialize(source);
            if (!_cases.ContainsKey(code))
            {
                throw new KeyNotFoundException(code.ToString());
            }
            return _cases[code].Deserialize(source);
        }
        public void Serialize(T o, Stream sink)
        {
            foreach (var pair in _cases)
            {
                if (pair.Value.canSerialize(o))
                {
                    codeFormatter.Serialize(pair.Key, sink);
                    pair.Value.Serialize(o, sink);
                    return;
                }
            }
            throw new Exception("no case can serialize object");
        }
        public int SerializeSize(T o)
        {
            return -1;
        }
        public bool isGreedyDeserialize { get; } = false;
    }
}
