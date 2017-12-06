using System;
using System.IO;

namespace CipherStone
{
    public abstract class SelectFormatter<TOuter, TInner> : IFormatter<TOuter>
    {
        private readonly IFormatter<TInner> _innerSerializer;
        protected abstract TInner ToInner(TOuter outer);
        protected abstract TOuter ToOuter(TInner inner);
        private readonly bool _mapForSize;
        protected SelectFormatter(IFormatter<TInner> innerSerializer, bool mapForSize = false)
        {
            _innerSerializer = innerSerializer;
            _mapForSize = mapForSize;
        }
        public TOuter Deserialize(Stream source)
        {
            return ToOuter(_innerSerializer.Deserialize(source));
        }
        public void Serialize(TOuter o, Stream sink)
        {
            _innerSerializer.Serialize(ToInner(o), sink);
        }
        public int SerializeSize(TOuter o)
        {
            if (_mapForSize)
                return _innerSerializer.SerializeSize(ToInner(o));
            return -1;
        }
        public bool isGreedyDeserialize => _innerSerializer.isGreedyDeserialize;
    }
    public class SelectFuncFormatter<TOuter, TInner> : SelectFormatter<TOuter, TInner>
    {
        private readonly Func<TOuter, TInner> _toInner;
        private readonly Func<TInner, TOuter> _toOuter;
        public SelectFuncFormatter(IFormatter<TInner> innerSerializer, Func<TOuter, TInner> toInner, Func<TInner, TOuter> toOuter, bool mapForSize = false):
            base(innerSerializer, mapForSize)
        {
            _toInner = toInner;
            _toOuter = toOuter;
        }
        protected override TInner ToInner(TOuter outer)
        {
            return _toInner(outer);
        }
        protected override TOuter ToOuter(TInner inner)
        {
            return _toOuter(inner);
        }
    }
}
