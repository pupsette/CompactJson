using System.Reflection;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    sealed class ConverterParameters
    {
        public ConverterParameters(IConverterFactory elementConverterFactory)
        {
            ElementConverterFactory = elementConverterFactory;
        }

        public IConverterFactory ElementConverterFactory { get; }

        public static ConverterParameters Reflect(MemberInfo memberInfo)
        {
            IConverterFactory elementConverter = ConverterFactoryHelper.FromType(ElementConverterAttribute.GetConverterType(memberInfo));
            return elementConverter != null ? new ConverterParameters(elementConverter) : null;
        }
    }
}
