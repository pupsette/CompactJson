using System;

namespace CompactJson
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#endif
    class EmitDefaultValueAttribute : Attribute
    {
    }
}
