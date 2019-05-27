using System;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypedConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return TypeNameAttribute.GetKnownTypes(type) != null;
        }

        public IConverter Create(Type type, ConverterParameters parameters)
        {
            return new TypedConverter(type);
        }
    }
}
