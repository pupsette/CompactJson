using System;

namespace CompactJson
{
    /// <summary>
    /// An attribute for properties or fields to control, whether
    /// the default value should be emitted or not, when writing to
    /// a <see cref="IJsonConsumer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class EmitDefaultValueAttribute : Attribute
    {
    }
}
