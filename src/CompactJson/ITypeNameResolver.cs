using System;

namespace CompactJson
{
    /// <summary>
    /// An interface for resolving type names for .NET types
    /// in an object hierarchy. This is used in conjunction with the
    /// 
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface ITypeNameResolver
    {
        bool TryGetTypeName(Type type, out string typeName, out IConverter converter);
        bool TryGetConverterFromTypeName(string typeName, out Type type, out IConverter converter);
    }
}
