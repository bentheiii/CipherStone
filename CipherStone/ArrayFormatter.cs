using System.IO;
using WhetStone.Looping;

namespace CipherStone
{
    public class ArrayFormatter<T> : IFormatter<T[]>
    {
        private readonly IFormatter<T> _inner;
        public ArrayFormatter(IFormatter<T> inner = null)
        {
            inner = inner ?? getFormatter.GetFormatter<T>();
            _inner = inner.EnsureNonGreedy();
        }
        public T[] Deserialize(Stream source)
        {
            ResizingArray<T> ret = new ResizingArray<T>();
            while (source.Position < source.Length)
            {
                ret.Add(_inner.Deserialize(source));
            }
            return ret.arr;
        }
        public void Serialize(T[] o, Stream sink)
        {
            foreach (var t in o)
            {
                _inner.Serialize(t, sink);
            }
        }
        public int SerializeSize(T[] o)
        {
            var sizes = o.Select(_inner.SerializeSize);
            var (anyNeg, sum) = sizes.Tally()
                .TallyAny(a => a < 0, @break: true)
                .TallyAggregate((a, b) => a + b,0)
                .Do();
            if (anyNeg)
                return -1;
            return sum;
        }
        public bool isGreedyDeserialize => true;
    }
}
