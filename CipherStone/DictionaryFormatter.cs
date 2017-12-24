using System.Collections.Generic;
using System.Linq;
using WhetStone.Looping;

namespace CipherStone
{
    public class DictionaryFormatter<K,V> : SelectFormatter<Dictionary<K,V>, (K,V)[]>
    {
        protected override (K, V)[] ToInner(Dictionary<K, V> outer)
        {
            return outer.Select(a => (a.Key, a.Value)).ToArray();
        }
        protected override Dictionary<K, V> ToOuter((K, V)[] inner)
        {
            var ret = new Dictionary<K,V>();
            foreach (var (key, value) in inner)
            {
                ret[key] = value;
            }
            return ret;
        }
        public DictionaryFormatter(IFormatter<(K, V)[]> innerSerializer) :
            base(innerSerializer)
        {}
        public DictionaryFormatter(IFormatter<K> keySerializer, IFormatter<V> valueSerializer):
            this(new ArrayFormatter<(K, V)>(new TupleFormatter<K, V>(keySerializer, valueSerializer)))
        {}
    }
}
