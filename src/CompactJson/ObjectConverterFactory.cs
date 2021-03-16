using System;

namespace CompactJson
{
    internal sealed class ObjectConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return type.IsClass || (type.IsValueType && !type.IsPrimitive);
        }

        public IConverter Create(Type type, object[] parameters)
        {
            return new ObjectConverter(type);
        }
    }
}
