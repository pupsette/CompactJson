using System;
using System.Reflection;

namespace CompactJson
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class CustomConverterAttribute : Attribute
    {
        public CustomConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        public Type ConverterType { get; }

        internal static Type GetConverterType(Type type)
        {
            CustomConverterAttribute att = type.GetCustomAttribute<CustomConverterAttribute>(true);
            if (att == null)
                return null;
            if (att.ConverterType == null)
                throw new Exception($"{nameof(ConverterType)} must not be null in {nameof(CustomConverterAttribute)} for type {type.Name}.");
            return att.ConverterType;
        }

        internal static Type GetConverterType(MemberInfo memberInfo)
        {
            CustomConverterAttribute att = memberInfo.GetCustomAttribute<CustomConverterAttribute>(true);
            if (att == null)
                return null;
            if (att.ConverterType == null)
                throw new Exception($"{nameof(ConverterType)} must not be null in {nameof(CustomConverterAttribute)} for member {memberInfo.Name} of type {memberInfo.ReflectedType}.");
            return att.ConverterType;
        }
    }
}
