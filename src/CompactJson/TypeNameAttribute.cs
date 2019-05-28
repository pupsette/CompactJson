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
    /// The <see cref="TypeNameAttribute"/> has to be added multiple
    /// times to the base class. Once for each sub class, it should support.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class TypeNameAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a <see cref="TypeNameAttribute"/>.
        /// </summary>
        /// <param name="type">The known type for which to assign a type name.</param>
        /// <param name="typeName">The type name to assign.</param>
        public TypeNameAttribute(Type type, string typeName)
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

        internal static TypeNameAttribute[] GetKnownTypes(Type type)
        {
            IEnumerable<TypeNameAttribute> atts = type.GetCustomAttributes<TypeNameAttribute>(false);
            if (atts == null)
                return null;

            TypeNameAttribute[] result = atts.ToArray();
            if (result.Length == 0)
                return null;

            foreach (TypeNameAttribute att in atts)
            {
                if (att.Type == null)
                    throw new Exception($"{nameof(Type)} must not be null in {nameof(TypeNameAttribute)} for type {type.Name}.");
                if (string.IsNullOrEmpty(att.TypeName))
                    throw new Exception($"{nameof(TypeName)} must not be null or empty in {nameof(TypeNameAttribute)} for type {type.Name}.");
            }
            return result;
        }
    }
}
