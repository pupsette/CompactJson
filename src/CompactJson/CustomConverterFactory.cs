using System;

namespace CompactJson
{
    /// <summary>
    /// This converter factory checks for the <see cref="CustomConverterAttribute"/>.
    /// If found a converter is instantiated according to the type specified.
    /// </summary>
    internal sealed class CustomConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return Attribute.IsDefined(type, typeof(CustomConverterAttribute), true);
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
