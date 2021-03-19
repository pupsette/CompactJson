using System;

namespace CompactJson
{
    /// <summary>
    /// This converter factory checks for the <see cref="JsonCustomConverterAttribute"/>.
    /// If found a converter is instantiated according to the type specified.
    /// </summary>
    internal sealed class CustomConverterFactory : IConverterFactory
    {
        public bool CanConvert(Type type)
        {
            return Attribute.IsDefined(type, typeof(JsonCustomConverterAttribute), true);
        }

        public IConverter Create(Type type, object[] converterParameters)
        {
            // if a custom converter has been specified using the CustomConverterAttribute,
            //  we use this converter instead.
            Type customConverterType = JsonCustomConverterAttribute.GetConverterType(type, out object[] parametersAtType);
            if (customConverterType == null)
                throw new Exception($"The {nameof(CustomConverterFactory)} cannot be used for type {type}, because the {nameof(JsonCustomConverterAttribute)} is not defined for this type.");

            return ConverterFactoryHelper.CreateConverter(customConverterType, type, converterParameters ?? parametersAtType);
        }
    }
}
