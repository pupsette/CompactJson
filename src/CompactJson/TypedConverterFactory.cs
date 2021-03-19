using System;

namespace CompactJson
{
    /// <summary>
    /// A converter factory which can be used to encode type information
    /// into JSON objects.
    /// This is helpful to support serialization and deserialization
    /// of class hierarchies.
    /// If class 'A' is base class of class 'B' you may add <see cref="JsonTypeNameAttribute"/>s
    /// to class 'A' to assign names to these classes.
    /// Additionally, add the <see cref="TypedConverterFactory"/> using the
    /// <see cref="JsonCustomConverterAttribute"/>.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypedConverterFactory : IConverterFactory
    {
        /// <summary>
        /// Checks, whether this converter factory is capable of converting the
        /// given type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>true, if this converter factory can create a converter for
        /// the given type; false, otherwise.</returns>
        public bool CanConvert(Type type)
        {
            return JsonTypeNameAttribute.GetKnownTypes(type) != null;
        }

        /// <summary>
        /// Creates a converter for the given type. This converter factory
        /// takes one optional parameter for the name of the type property.
        /// This defaults to 'Type'.
        /// </summary>
        /// <param name="type">The type for which to create the converter.</param>
        /// <param name="parameters">An optional set of converter-specific parameters 
        /// for the new converter.</param>
        /// <returns>An instance of <see cref="IConverter"/> which is capable
        /// of converting objects of the given type.</returns>
        public IConverter Create(Type type, object[] parameters)
        {
            string typeProperty = ConverterFactoryHelper.GetConverterParameter<string>(typeof(TypedConverterFactory), parameters, 0, 0, 1);
            return new TypedConverter(type, typeProperty ?? "Type");
        }
    }
}
