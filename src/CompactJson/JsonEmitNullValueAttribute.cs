using System;

namespace CompactJson
{
    /// <summary>
    /// An attribute for properties or fields to control, whether
    /// null values should be emitted or not, when writing to
    /// a <see cref="IJsonConsumer"/>. By default, null values will not
    /// be emitted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonEmitNullValueAttribute : Attribute
    {
    }
}
