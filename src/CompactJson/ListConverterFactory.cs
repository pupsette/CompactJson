using System;
using System.Collections.Generic;

namespace CompactJson
{
    internal sealed class ListConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(List<>) ||
                type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public IConverter Create(Type type, object[] converterParameters)
        {
            Type genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef != typeof(List<>) && genericTypeDef != typeof(IList<>))
                throw new ArgumentException($"Type '{type}' was expected to be a generic List<> or IList<>.");

            Type elementType = type.GetGenericArguments()[0];
            Type elementConverterType = ConverterFactoryHelper.GetConverterParameter<Type>(typeof(ListConverterFactory), converterParameters, 0, 0, 1);
            IConverter elementConverter = ConverterFactoryHelper.CreateConverter(elementConverterType, elementType, null);

            Type listConverterType = typeof(ListConverter<>).MakeGenericType(elementType);
            return (IConverter)Activator.CreateInstance(listConverterType, new object[] { elementConverter });
        }
    }
}
