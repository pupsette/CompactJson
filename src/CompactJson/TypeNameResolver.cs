using System;
using System.Collections.Generic;

namespace CompactJson
{
    /// <summary>
    /// Implementation of <see cref="ITypeNameResolver"/>.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypeNameResolver : ITypeNameResolver
    {
        /// <summary>
        /// Initializes a new <see cref="TypeNameResolver"/>.
        /// </summary>
        /// <param name="caseSensitive">Whether the type names are case-sensitive or not.</param>
        public TypeNameResolver(bool caseSensitive = false)
        {
            mNameToType = new Dictionary<string, Entry>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            mTypeToName = new Dictionary<Type, Entry>();
        }

        /// <summary>
        /// Assign the specified type name to the given type. Each type and each
        /// type name must only occur once!
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <param name="type">The type.</param>
        public void AddType(string typeName, Type type)
        {
            Entry entry;
            entry.TypeName = typeName;
            entry.Type = type;

            mNameToType.Add(typeName, entry);
            mTypeToName.Add(type, entry);
        }

        /// <summary>
        /// Tries to find the type name for the given type. If the type
        /// is unknown, false is returned.
        /// </summary>
        /// <param name="type">The type for which to find the type name.</param>
        /// <param name="typeName">The resulting type name, or null.</param>
        /// <returns>true, if a type name was assigned to the given type; false, otherwise.</returns>
        public bool TryGetTypeName(Type type, out string typeName)
        {
            if (mTypeToName.TryGetValue(type, out Entry entry))
            {
                typeName = entry.TypeName;
                return true;
            }
            else
            {
                typeName = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to find the type for the given type name. If the type name
        /// is unknown, false is returned.
        /// </summary>
        /// <param name="typeName">The type name for which to find the type.</param>
        /// <param name="type">The resulting type, or null.</param>
        /// <returns>true, if the type name is known; false, otherwise.</returns>
        public bool TryGetType(string typeName, out Type type)
        {
            if (mNameToType.TryGetValue(typeName, out Entry entry))
            {
                type = entry.Type;
                return true;
            }
            else
            {
                type = null;
                return false;
            }
        }

        /// <summary>
        /// An optional default type, which can be deserialized in case the type
        /// property is missing. The type name will also not be serialized for this
        /// default type.
        /// </summary>
        public Type DefaultType { get; set; }


        private struct Entry
        {
            public Type Type;
            public string TypeName;
        }

        private readonly Dictionary<string, Entry> mNameToType;
        private readonly Dictionary<Type, Entry> mTypeToName;
    }
}
