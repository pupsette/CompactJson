using System;
using System.Reflection;

namespace CompactJson
{
    /// <summary>
    /// An attribute for specifying the converter for a type or member
    /// explcitly.
    /// 
    /// This attribute may either reference an implementation of <see cref="IConverter"/>
    /// or an implementation of <see cref="IConverterFactory"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class CustomConverterAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a <see cref="CustomConverterAttribute"/>.
        /// </summary>
        /// <param name="converterType">The type which implements either 
        /// <see cref="IConverter"/> or <see cref="IConverterFactory"/>.</param>
        public CustomConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        /// <summary>
        /// The type which implements either <see cref="IConverter"/> or <see cref="IConverterFactory"/>.
        /// </summary>
        public Type ConverterType { get; }

        internal static Type GetConverterType(Type type)
        {
            CustomConverterAttribute att = type.GetCustomAttribute<CustomConverterAttribute>(false);
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
