using System;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class ArrayConverter<T> : CollectionConverterBase
    {
        public ArrayConverter(IConverter elementConverter) : base(typeof(T[]), elementConverter)
        {
        }

        public override object FromNull()
        {
            return null;
        }

        public override IJsonArrayConsumer FromArray(Action<object> whenDone)
        {
            return new ListConverter<T>.Consumer(ElementConverter, list => whenDone(list.ToArray()));
        }
    }
}
