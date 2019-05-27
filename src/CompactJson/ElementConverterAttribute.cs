using System;
using System.Reflection;

namespace CompactJson
{
    /// <summary>
    /// An attribute which can be used to use a specific converter for the
    /// target field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class ElementConverterAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ElementConverterAttribute"/> with the
        /// given converter type.
        /// </summary>
        /// <param name="converterType">The converter type which implements either
        /// <see cref="IConverterFactory"/> or <see cref="IConverter"/>. The given type must have
        /// a default constructor, too.</param>
        public ElementConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }

        /// <summary>
        /// The converter type which implements either <see cref="IConverterFactory"/> or <see cref="IConverter"/>.
        /// </summary>
        public Type ConverterType { get; }

        /// <summary>
        /// Resolves the converter type for <paramref name="memberInfo"/>, if the attribute
        /// is present.
        /// </summary>
        /// <param name="memberInfo">The member info to inspect.</param>
        /// <returns>The converter type or null, if the attribute is not present.</returns>
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
