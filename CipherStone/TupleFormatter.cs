using System.IO;
// ReSharper disable PossibleInfiniteInheritance

namespace CipherStone
{
    public class TupleFormatter<T1, T2> : IFormatter<(T1,T2)>
    {
        private readonly IFormatter<T1> _inner1;
        private readonly IFormatter<T2> _inner2;
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null)
        {
            inner1 = inner1 ?? getFormatter.GetFormatter<T1>();
            inner2 = inner2 ?? getFormatter.GetFormatter<T2>();
            _inner1 = inner1.EnsureNonGreedy();
            _inner2 = inner2.EnsureNonGreedy();
        }
        public (T1, T2) Deserialize(Stream source)
        {
            T1 t1 = _inner1.Deserialize(source);
            T2 t2 = _inner2.Deserialize(source);
            return (t1, t2);
        }
        public void Serialize((T1, T2) o, Stream sink)
        {
            _inner1.Serialize(o.Item1, sink);
            _inner2.Serialize(o.Item2, sink);
        }
        public int SerializeSize((T1, T2) o)
        {
            var l1 = _inner1.SerializeSize(o.Item1);
            if (l1 < 0)
                return -1;
            var l2 = _inner2.SerializeSize(o.Item2);
            if (l2 < 0)
                return -1;
            return l1 + l2;
        }
        public bool isGreedyDeserialize => false;
    }
    public class TupleFormatter<T1, T2, T3> : SelectFormatter<(T1, T2, T3), ((T1, T2), T3)>
    {
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null, IFormatter<T3> inner3 = null) :
            base(
                new TupleFormatter<(T1, T2), T3>(new TupleFormatter<T1, T2>(inner1, inner2), inner3),
                true
            )
        {}
        protected override ((T1, T2), T3) ToInner((T1, T2, T3) outer)
        {
            return ((outer.Item1, outer.Item2), outer.Item3);
        }
        protected override (T1, T2, T3) ToOuter(((T1, T2), T3) inner)
        {
            return (inner.Item1.Item1, inner.Item1.Item2, inner.Item2);
        }
    }
    public class TupleFormatter<T1, T2, T3, T4> : SelectFormatter<(T1, T2, T3, T4), ((T1, T2), (T3, T4))>
    {
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null,
                               IFormatter<T3> inner3 = null, IFormatter<T4> inner4 = null) :
            base(
                new TupleFormatter<(T1, T2), (T3,T4)>(new TupleFormatter<T1, T2>(inner1, inner2), new TupleFormatter<T3, T4>(inner3, inner4)),
                true
            )
        { }
        protected override ((T1, T2), (T3,T4)) ToInner((T1, T2, T3, T4) outer)
        {
            return ((outer.Item1, outer.Item2), (outer.Item3, outer.Item4));
        }
        protected override (T1, T2, T3, T4) ToOuter(((T1, T2), (T3, T4)) inner)
        {
            return (inner.Item1.Item1, inner.Item1.Item2, inner.Item2.Item1, inner.Item2.Item2);
        }
    }
    public class TupleFormatter<T1, T2, T3, T4, T5> : SelectFormatter<(T1, T2, T3, T4, T5), ((T1, T2), (T3, T4, T5))>
    {
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null,
            IFormatter<T3> inner3 = null, IFormatter<T4> inner4 = null, IFormatter<T5> inner5 = null) :
            base(
                new TupleFormatter<(T1, T2), (T3, T4, T5)>(new TupleFormatter<T1, T2>(inner1, inner2), new TupleFormatter<T3, T4, T5>(inner3, inner4, inner5)),
                true
            )
        { }
        protected override ((T1, T2), (T3, T4, T5)) ToInner((T1, T2, T3, T4, T5) outer)
        {
            return ((outer.Item1, outer.Item2), (outer.Item3, outer.Item4, outer.Item5));
        }
        protected override (T1, T2, T3, T4, T5) ToOuter(((T1, T2), (T3, T4, T5)) inner)
        {
            return (inner.Item1.Item1, inner.Item1.Item2, inner.Item2.Item1, inner.Item2.Item2, inner.Item2.Item3);
        }

    }
    public class TupleFormatter<T1, T2, T3, T4, T5, T6> : SelectFormatter<(T1, T2, T3, T4, T5, T6), ((T1, T3, T5), (T2, T4, T6))>
    {
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null,
            IFormatter<T3> inner3 = null, IFormatter<T4> inner4 = null, IFormatter<T5> inner5 = null,
            IFormatter<T6> inner6 = null) :
            base(
                new TupleFormatter<(T1, T3, T5), (T2, T4, T6)>(new TupleFormatter<T1, T3, T5>(inner1, inner3, inner5), new TupleFormatter<T2, T4, T6>(inner2, inner4, inner6)),
                true
            )
        { }
        protected override ((T1, T3, T5), (T2, T4, T6)) ToInner((T1, T2, T3, T4, T5, T6) outer)
        {
            return ((outer.Item1, outer.Item3, outer.Item5), (outer.Item2, outer.Item4, outer.Item6));
        }
        protected override (T1, T2, T3, T4, T5, T6) ToOuter(((T1, T3, T5), (T2, T4, T6)) inner)
        {
            return (inner.Item1.Item1, inner.Item2.Item1, inner.Item1.Item2, inner.Item2.Item2, inner.Item1.Item3, inner.Item2.Item3);
        }
    }
    public class TupleFormatter<T1, T2, T3, T4, T5, T6, T7> : SelectFormatter<(T1, T2, T3, T4, T5, T6, T7), ((T1, T3, T5, T7), (T2, T4, T6))>
    {
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null,
            IFormatter<T3> inner3 = null, IFormatter<T4> inner4 = null, IFormatter<T5> inner5 = null,
            IFormatter<T6> inner6 = null, IFormatter<T7> inner7 = null) :
            base(
                new TupleFormatter<(T1, T3, T5, T7), (T2, T4, T6)>(new TupleFormatter<T1, T3, T5, T7>(inner1, inner3, inner5, inner7), new TupleFormatter<T2, T4, T6>(inner2, inner4, inner6)),
                true
            )
        { }
        protected override ((T1, T3, T5, T7), (T2, T4, T6)) ToInner((T1, T2, T3, T4, T5, T6, T7) outer)
        {
            return ((outer.Item1, outer.Item3, outer.Item5, outer.Item7), (outer.Item2, outer.Item4, outer.Item6));
        }
        protected override (T1, T2, T3, T4, T5, T6, T7) ToOuter(((T1, T3, T5, T7), (T2, T4, T6)) inner)
        {
            return (inner.Item1.Item1, inner.Item2.Item1, inner.Item1.Item2, inner.Item2.Item2, inner.Item1.Item3, inner.Item2.Item3, inner.Item1.Item4);
        }
    }
    public class TupleFormatter<T1, T2, T3, T4, T5, T6, T7, T8> : SelectFormatter<(T1, T2, T3, T4, T5, T6, T7, T8), ((T1, T3, T5, T7), (T2, T4, T6, T8))>
    {
        public TupleFormatter(IFormatter<T1> inner1 = null, IFormatter<T2> inner2 = null,
            IFormatter<T3> inner3 = null, IFormatter<T4> inner4 = null, IFormatter<T5> inner5 = null,
            IFormatter<T6> inner6 = null, IFormatter<T7> inner7 = null, IFormatter<T8> inner8 = null) :
            base(
                new TupleFormatter<(T1, T3, T5, T7), (T2, T4, T6, T8)>(new TupleFormatter<T1, T3, T5, T7>(inner1, inner3, inner5, inner7), new TupleFormatter<T2, T4, T6, T8>(inner2, inner4, inner6, inner8)),
                true
            )
        { }
        protected override ((T1, T3, T5, T7), (T2, T4, T6, T8)) ToInner((T1, T2, T3, T4, T5, T6, T7, T8) outer)
        {
            return ((outer.Item1, outer.Item3, outer.Item5, outer.Item7), (outer.Item2, outer.Item4, outer.Item6, outer.Item8));
        }
        protected override (T1, T2, T3, T4, T5, T6, T7, T8) ToOuter(((T1, T3, T5, T7), (T2, T4, T6, T8)) inner)
        {
            return (inner.Item1.Item1, inner.Item2.Item1, inner.Item1.Item2, inner.Item2.Item2, inner.Item1.Item3, inner.Item2.Item3, inner.Item1.Item4, inner.Item2.Item4);
        }
    }
}
