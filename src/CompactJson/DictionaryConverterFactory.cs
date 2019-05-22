using System;
using System.Collections.Generic;

namespace CompactJson
{
    internal sealed class DictionaryConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            if (!type.IsGenericType)
                return false;
            Type genericTypeDef = type.GetGenericTypeDefinition();
            return genericTypeDef == typeof(Dictionary<,>) && genericTypeDef.GetGenericArguments()[0] == typeof(string);
        }

        public IConverter Create(Type type, ConverterParameters converterParameters)
        {
            Type genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef != typeof(Dictionary<,>))
                throw new ArgumentException($"Type '{type}' was expected to be a generic Dictionary<,>.");

            Type[] genericArguments = type.GetGenericArguments();
            if (genericArguments[0] != typeof(string))
                throw new ArgumentException($"Type '{type}' was expected to have string keys.");

            Type elementType = genericArguments[1];
            Type dictConverterType = typeof(DictionaryConverter<>).MakeGenericType(elementType);
            return (IConverter)Activator.CreateInstance(dictConverterType, ConverterRegistry.Get(elementType, null));
        }
    }
}
