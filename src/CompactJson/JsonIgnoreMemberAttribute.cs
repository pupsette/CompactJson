using System;

namespace CompactJson
{
    /// <summary>
    /// An attribute which can be used to ignore members
    /// during serialization/deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonIgnoreMemberAttribute : Attribute
    {
    }
}
