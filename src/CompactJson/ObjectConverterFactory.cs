using System;
using System.Collections.Generic;

namespace CompactJson
{
    internal sealed class ObjectConverterFactory : IConverterFactory
    {
        // this dictionary prevents infinite recursion if properties have the same type as one
        //  of the declaring types in the object hierarchy.
        private static readonly Dictionary<Type, ObjectConverter> CURRENTLY_REFLECTED_TYPES = new Dictionary<Type, ObjectConverter>();

        public bool CanConvert(Type type)
        {
            return type.IsClass || (type.IsValueType && !type.IsPrimitive);
        }

        public IConverter Create(Type type, object[] parameters)
        {
            ObjectConverter converter;
            lock (CURRENTLY_REFLECTED_TYPES)
            {
                if (CURRENTLY_REFLECTED_TYPES.TryGetValue(type, out converter))
                    return converter;

                CURRENTLY_REFLECTED_TYPES[type] = converter = new ObjectConverter(type);
            }
            try
            {
                converter.Reflect();
            }
            finally
            {
                lock (CURRENTLY_REFLECTED_TYPES)
                    CURRENTLY_REFLECTED_TYPES.Remove(type);
            }
            return converter;
        }
    }
}
