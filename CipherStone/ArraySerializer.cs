using System.IO;
using WhetStone.Looping;

namespace CipherStone
{
    public class ArraySerializer<T> : IByteSerializer<T[]>
    {
        private readonly IByteSerializer<T> _inner;
        public ArraySerializer(IByteSerializer<T> inner)
        {
            _inner = inner.EnsureNonGreedy();
        }
        public T[] deserialize(Stream source)
        {
            ResizingArray<T> ret = new ResizingArray<T>();
            while (source.Position < source.Length)
            {
                ret.Add(_inner.deserialize(source));
            }
            return ret.arr;
        }
        public void serialize(T[] o, Stream sink)
        {
            foreach (var t in o)
            {
                _inner.serialize(t, sink);
            }
        }
        public int serializeSize(T[] o)
        {
            var sizes = o.Select(_inner.serializeSize);
            var (anyNeg, sum) = sizes.Tally().TallyAny(a => a < 0, @break: true).TallyAggregate((a, b) => a + b,0).Do();
            if (anyNeg)
                return -1;
            return sum;
        }
        public bool isGreedyDeserialize => true;
    }
}
