using System.IO;

namespace CipherStone
{
    public class TupleSerializer<T1, T2> : IByteSerializer<(T1,T2)>
    {
        private readonly IByteSerializer<T1> _inner1;
        private readonly IByteSerializer<T2> _inner2;
        public TupleSerializer(IByteSerializer<T1> inner1, IByteSerializer<T2> inner2)
        {
            _inner1 = inner1.EnsureNonGreedy();
            _inner2 = inner2.EnsureNonGreedy();
        }
        public (T1, T2) deserialize(Stream source)
        {
            T1 t1 = _inner1.deserialize(source);
            T2 t2 = _inner2.deserialize(source);
            return (t1, t2);
        }
        public void serialize((T1, T2) o, Stream sink)
        {
            _inner1.serialize(o.Item1, sink);
            _inner2.serialize(o.Item2, sink);
        }
        public int serializeSize((T1, T2) o)
        {
            var l1 = _inner1.serializeSize(o.Item1);
            var l2 = _inner2.serializeSize(o.Item2);
            if (l1 < 0 || l2 < 0)
                return -1;
            return l1 + l2;
        }
        public bool isGreedyDeserialize => false;
    }
    public class TupleSerializer<T1, T2, T3> : IByteSerializer<(T1, T2, T3)>
    {
        private readonly IByteSerializer<T1> _inner1;
        private readonly IByteSerializer<T2> _inner2;
        private readonly IByteSerializer<T3> _inner3;
        public TupleSerializer(IByteSerializer<T1> inner1, IByteSerializer<T2> inner2, IByteSerializer<T3> inner3)
        {
            _inner1 = inner1.EnsureNonGreedy();
            _inner2 = inner2.EnsureNonGreedy();
            _inner3 = inner3.EnsureNonGreedy();
        }
        public (T1, T2, T3) deserialize(Stream source)
        {
            T1 t1 = _inner1.deserialize(source);
            T2 t2 = _inner2.deserialize(source);
            T3 t3 = _inner3.deserialize(source);
            return (t1, t2, t3);
        }
        public void serialize((T1, T2, T3) o, Stream sink)
        {
            _inner1.serialize(o.Item1, sink);
            _inner2.serialize(o.Item2, sink);
            _inner3.serialize(o.Item3, sink);
        }
        public int serializeSize((T1, T2, T3) o)
        {
            var l1 = _inner1.serializeSize(o.Item1);
            var l2 = _inner2.serializeSize(o.Item2);
            var l3 = _inner3.serializeSize(o.Item3);
            if (l1 < 0 || l2 < 0 || l3 < 0)
                return -1;
            return l1 + l2 + l3;
        }
        public bool isGreedyDeserialize => false;
    }
    public class TupleSerializer<T1, T2, T3, T4> : IByteSerializer<(T1, T2, T3, T4)>
    {
        private readonly IByteSerializer<T1> _inner1;
        private readonly IByteSerializer<T2> _inner2;
        private readonly IByteSerializer<T3> _inner3;
        private readonly IByteSerializer<T4> _inner4;
        public TupleSerializer(IByteSerializer<T1> inner1, IByteSerializer<T2> inner2, IByteSerializer<T3> inner3, IByteSerializer<T4> inner4)
        {
            _inner1 = inner1.EnsureNonGreedy();
            _inner2 = inner2.EnsureNonGreedy();
            _inner3 = inner3.EnsureNonGreedy();
            _inner4 = inner4.EnsureNonGreedy();
        }
        public (T1, T2, T3, T4) deserialize(Stream source)
        {
            T1 t1 = _inner1.deserialize(source);
            T2 t2 = _inner2.deserialize(source);
            T3 t3 = _inner3.deserialize(source);
            T4 t4 = _inner4.deserialize(source);
            return (t1, t2, t3, t4);
        }
        public void serialize((T1, T2, T3, T4) o, Stream sink)
        {
            _inner1.serialize(o.Item1, sink);
            _inner2.serialize(o.Item2, sink);
            _inner3.serialize(o.Item3, sink);
            _inner4.serialize(o.Item4, sink);
        }
        public int serializeSize((T1, T2, T3, T4) o)
        {
            var l1 = _inner1.serializeSize(o.Item1);
            var l2 = _inner2.serializeSize(o.Item2);
            var l3 = _inner3.serializeSize(o.Item3);
            var l4 = _inner4.serializeSize(o.Item4);
            if (l1 < 0 || l2 < 0 || l3 < 0 || l4<0)
                return -1;
            return l1 + l2 + l3 + l4;
        }
        public bool isGreedyDeserialize => false;
    }
}
