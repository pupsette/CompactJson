using System.Reflection;

namespace CompactJson
{
    /// <summary>
    /// A set of parameters for converters which may be used
    /// during converter construction. These parameters are fully derived
    /// from the attributes of a property or field. Currently, only the
    /// <see cref="ElementConverterAttribute"/> is considered.
    /// 
    /// The mechanism currently passes these parameters to converter
    /// factories, when attributes are present for a property or field.
    /// This happens regardless whether the converter is able to use these
    /// parameters or not.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    sealed class ConverterParameters
    {
        /// <summary>
        /// Instantiates converter parameters.
        /// </summary>
        /// <param name="elementConverterFactory">The converter for child elements.</param>
        public ConverterParameters(IConverterFactory elementConverterFactory)
        {
            ElementConverterFactory = elementConverterFactory;
        }

        /// <summary>
        /// The converter for child elements.
        /// </summary>
        public IConverterFactory ElementConverterFactory { get; }

        /// <summary>
        /// Creates an instance of <see cref="ConverterParameters"/> in case 
        /// attributes are present. If no attributes are present (no parameters)
        /// this method returns null.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> to retrieve
        /// the attributes from.</param>
        /// <returns>The converter parameters, if present; null, otherwise.</returns>
        public static ConverterParameters Reflect(MemberInfo memberInfo)
        {
            IConverterFactory elementConverter = ConverterFactoryHelper.FromType(ElementConverterAttribute.GetConverterType(memberInfo));
            return elementConverter != null ? new ConverterParameters(elementConverter) : null;
        }
    }
}
