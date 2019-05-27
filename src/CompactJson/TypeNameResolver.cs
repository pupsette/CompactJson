using System;
using System.Collections.Generic;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypeNameResolver : ITypeNameResolver
    {
        public TypeNameResolver(bool caseSensitive = false)
        {
            mNameToType = new Dictionary<string, Entry>(caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase);
            mTypeToName = new Dictionary<Type, Entry>();
        }

        public void AddType(string typeName, Type type, IConverter customConverter = null)
        {
            Entry entry;
            entry.TypeName = typeName;
            entry.Converter = customConverter ?? ConverterRegistry.Get(type);
            entry.Type = type;

            mNameToType.Add(typeName, entry);
            mTypeToName.Add(type, entry);
        }

        public bool TryGetTypeName(Type type, out string typeName, out IConverter converter)
        {
            if (mTypeToName.TryGetValue(type, out Entry entry))
            {
                typeName = entry.TypeName;
                converter = entry.Converter;
                return true;
            }
            else
            {
                typeName = null;
                converter = null;
                return false;
            }
        }

        public bool TryGetConverterFromTypeName(string typeName, out Type type, out IConverter converter)
        {
            if (mNameToType.TryGetValue(typeName, out Entry entry))
            {
                type = entry.Type;
                converter = entry.Converter;
                return true;
            }
            else
            {
                type = null;
                converter = null;
                return false;
            }
        }

        private struct Entry
        {
            public IConverter Converter;
            public Type Type;
            public string TypeName;
        }

        private readonly Dictionary<string, Entry> mNameToType;
        private readonly Dictionary<Type, Entry> mTypeToName;
    }
}
