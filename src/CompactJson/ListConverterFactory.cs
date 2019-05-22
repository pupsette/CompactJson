using System;
using System.Collections.Generic;
using System.Reflection;

namespace CompactJson
{
    internal class ListConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        public IConverter Create(Type type, ConverterParameters converterParameters)
        {
            Type genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef != typeof(List<>))
                throw new ArgumentException($"Type '{type}' was expected to be a generic List<>.");

            Type elementType = genericTypeDef.GetGenericArguments()[0];
            Type listConverterType = typeof(ListConverter<>).MakeGenericType(elementType);
            IConverter elementConverter = ConverterFactoryHelper.CreateConverter(converterParameters?.ElementConverterFactory, elementType, null);
            return (IConverter)Activator.CreateInstance(listConverterType, elementConverter);
        }
    }
}
