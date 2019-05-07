using System;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IConverterFactory
    {
        IConverter Create(Type type);
    }
}
