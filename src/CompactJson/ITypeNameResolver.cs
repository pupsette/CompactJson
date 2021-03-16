using System;

namespace CompactJson
{
    /// <summary>
    /// An interface for resolving type names for .NET types
    /// in an object hierarchy. This is used in conjunction with the
    /// <see cref="TypedConverter"/>.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface ITypeNameResolver
    {
        /// <summary>
        /// Tries to find the type name for the given type. If the type
        /// is unknown, false is returned.
        /// </summary>
        /// <param name="type">The type for which to find the type name.</param>
        /// <param name="typeName">The resulting type name, or null.</param>
        /// <returns>true, if a type name was assigned to the given type; false, otherwise.</returns>
        bool TryGetTypeName(Type type, out string typeName);

        /// <summary>
        /// Tries to find the type for the given type name. If the type name
        /// is unknown, false is returned.
        /// </summary>
        /// <param name="typeName">The type name for which to find the type.</param>
        /// <param name="type">The resulting type, or null.</param>
        /// <returns>true, if the type name is known; false, otherwise.</returns>
        bool TryGetType(string typeName, out Type type);

        /// <summary>
        /// An optional default type, which can be deserialized in case the type
        /// property is missing. The type name will also not be serialized for this
        /// default type.
        /// </summary>
        Type DefaultType { get; }
    }
}
