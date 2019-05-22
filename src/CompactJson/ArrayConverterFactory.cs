using System;
using System.Reflection;

namespace CompactJson
{
    internal class ArrayConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return type.IsArray;
        }

        public IConverter Create(Type type, ConverterParameters converterParameters)
        {
            if (!type.IsArray)
                throw new ArgumentException($"Type '{type}' was expected to be an array.");

            Type elementType = type.GetElementType();
            Type listConverterType = typeof(ArrayConverter<>).MakeGenericType(elementType);
            IConverter elementConverter = ConverterFactoryHelper.CreateConverter(converterParameters?.ElementConverterFactory, elementType, null);
            return (IConverter)Activator.CreateInstance(listConverterType, elementConverter);
        }
    }
}
