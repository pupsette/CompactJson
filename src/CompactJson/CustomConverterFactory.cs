using System;

namespace CompactJson
{
    internal sealed class CustomConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return Attribute.IsDefined(type, typeof(CustomConverterAttribute), false);
        }

        public IConverter Create(Type type, object[] converterParameters)
        {
            // if a custom converter has been specified using the CustomConverterAttribute,
            //  we use this converter instead.
            Type customConverterType = CustomConverterAttribute.GetConverterType(type, out object[] parametersAtType);
            if (customConverterType == null)
                throw new Exception($"The {nameof(CustomConverterFactory)} cannot be used for type {type}, because the {nameof(CustomConverterAttribute)} is not defined for this type.");

            return ConverterFactoryHelper.CreateConverter(customConverterType, type, converterParameters ?? parametersAtType);
        }
    }
}
