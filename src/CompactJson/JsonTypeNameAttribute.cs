using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CompactJson
{
    /// <summary>
    /// An attribute which is used to assign type names
    /// to .NET classes in case type information has to be encoded
    /// in JSON data by using the <see cref="TypedConverter"/>.
    /// 
    /// The <see cref="JsonTypeNameAttribute"/> has to be added multiple
    /// times to the base class. Once for each sub class, it should support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonTypeNameAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a <see cref="JsonTypeNameAttribute"/>.
        /// </summary>
        /// <param name="type">The known type for which to assign a type name.</param>
        /// <param name="typeName">The type name to assign. This can be left null for 
        /// a single type of the <see cref="TypedConverterFactory" /> to mark it as
        /// default type. This default type will be deserialized, if the type property 
        /// is missing in the source JSON. The type name will also not be 
        /// serialized for the default type.</param>
        public JsonTypeNameAttribute(Type type, string typeName)
        {
            Type = type;
            TypeName = typeName;
        }

        /// <summary>
        /// The assigned type name.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// The type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// A boolean flag, indicating, whether this instance will be deserialized, if the type property
        /// is missing. The type name will also not be serialized for the default type.
        /// </summary>
        public bool IsDefaultType { get => TypeName == null; }

        internal static JsonTypeNameAttribute[] GetKnownTypes(Type type)
        {
            IEnumerable<JsonTypeNameAttribute> atts = type.GetCustomAttributes<JsonTypeNameAttribute>(true);
            if (atts == null)
                return null;

            JsonTypeNameAttribute[] result = atts.ToArray();
            if (result.Length == 0)
                return null;

            foreach (JsonTypeNameAttribute att in atts)
            {
                if (att.Type == null)
                    throw new Exception($"{nameof(Type)} must not be null in {nameof(JsonTypeNameAttribute)} for type {type.Name}.");
            }
            return result;
        }
    }
}
