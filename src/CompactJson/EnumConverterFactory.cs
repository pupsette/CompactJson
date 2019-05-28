using System;

namespace CompactJson
{
    internal sealed class EnumConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return type.IsEnum;
        }

        public IConverter Create(Type type, object[] converterParameters)
        {
            if (!type.IsEnum)
                throw new ArgumentException($"Type '{type}' was expected to be an enum.");

            return new EnumConverter(type);
        }
    }
}
