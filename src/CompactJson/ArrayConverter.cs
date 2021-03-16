using System;

namespace CompactJson
{
    internal sealed class ArrayConverter<T> : CollectionConverterBase
    {
        public ArrayConverter(IConverter elementConverter) : base(typeof(T[]), () => elementConverter)
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
