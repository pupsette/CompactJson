using System;
using System.Reflection;

namespace CompactJson
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class ElementConverterAttribute : Attribute
    {
        public ElementConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        public Type ConverterType { get; }

        internal static Type GetConverterType(MemberInfo memberInfo)
        {
            CustomConverterAttribute att = memberInfo.GetCustomAttribute<CustomConverterAttribute>(true);
            if (att == null)
                return null;
            if (att.ConverterType == null)
                throw new Exception($"{nameof(ConverterType)} must not be null in {nameof(ElementConverterAttribute)} for member {memberInfo.Name} of type {memberInfo.ReflectedType}.");
            return att.ConverterType;
        }
    }
}
