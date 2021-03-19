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
    class JsonCustomConverterAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a <see cref="JsonCustomConverterAttribute"/>.
        /// </summary>
        /// <param name="converterType">The type which implements either 
        /// <see cref="IConverter"/> or <see cref="IConverterFactory"/>.</param>
        /// <param name="parameters">An optional set of parameters passed to the <see cref="IConverterFactory"/>.
        /// Note, that these parameters only work when used with a <see cref="IConverterFactory"/>. These
        /// parameters are not passed to non-default constructors of <see cref="IConverter"/>s.</param>
        public JsonCustomConverterAttribute(Type converterType, params object[] parameters)
        {
            ConverterType = converterType;
            ConverterParameters = parameters;
        }

        /// <summary>
        /// The type which implements either <see cref="IConverter"/> or <see cref="IConverterFactory"/>.
        /// </summary>
        public Type ConverterType { get; }

        /// <summary>
        /// An optional set of parameters passed to the <see cref="IConverterFactory"/>.
        /// Note, that these parameters only work when used with a <see cref="IConverterFactory"/>. These
        /// parameters are not passed to non-default constructors of <see cref="IConverter"/>s.
        /// </summary>
        public object[] ConverterParameters { get; }

        internal static Type GetConverterType(Type type, out object[] converterParameters)
        {
            converterParameters = null;
            JsonCustomConverterAttribute att = type.GetCustomAttribute<JsonCustomConverterAttribute>(true);
            if (att == null)
                return null;
            if (att.ConverterType == null)
                throw new Exception($"{nameof(ConverterType)} must not be null in {nameof(JsonCustomConverterAttribute)} for type {type.Name}.");
            converterParameters = att.GetParameters();
            return att.ConverterType;
        }

        internal static Type GetConverterType(MemberInfo memberInfo, out object[] converterParameters)
        {
            converterParameters = null;
            JsonCustomConverterAttribute att = memberInfo.GetCustomAttribute<JsonCustomConverterAttribute>(true);
            if (att == null)
                return null;
            if (att.ConverterType == null)
                throw new Exception($"{nameof(ConverterType)} must not be null in {nameof(JsonCustomConverterAttribute)} for member {memberInfo.Name} of type {memberInfo.ReflectedType}.");
            converterParameters = att.GetParameters();
            return att.ConverterType;
        }

        private object[] GetParameters()
        {
            if (ConverterParameters == null || ConverterParameters.Length == 0)
                return null;
            return ConverterParameters;
        }
    }
}
