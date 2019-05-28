using System;

namespace CompactJson
{
    internal class ArrayConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return type.IsArray;
        }

        public IConverter Create(Type type, object[] converterParameters)
        {
            if (!type.IsArray)
                throw new ArgumentException($"Type '{type}' was expected to be an array.");

            Type elementType = type.GetElementType();
            Type elementConverterType = ConverterFactoryHelper.GetConverterParameter<Type>(typeof(ArrayConverterFactory), converterParameters, 0, 0, 1);
            IConverter elementConverter = ConverterFactoryHelper.CreateConverter(elementConverterType, elementType, null);

            Type listConverterType = typeof(ArrayConverter<>).MakeGenericType(elementType);
            return (IConverter)Activator.CreateInstance(listConverterType, elementConverter);
        }
    }
}
