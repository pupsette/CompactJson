using System;

namespace CompactJson
{
    /// <summary>
    /// An attribute which can be used to include 
    /// private/internal/protected members during serialization/deserialization.
    /// Also it can be used to use a custom property name during serialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="JsonPropertyAttribute"/>.
        /// </summary>
        /// <param name="propertyName">The name of the property in JSON. If this is 
        /// not set, the name of the field or property is used.
        /// </param>
        public JsonPropertyAttribute(string propertyName = null)
        {
            Name = propertyName;
        }

        /// <summary>
        /// An optional name of the property. If this is not set, the name of the 
        /// field or property is used.
        /// </summary>
        public string Name { get; set; }
    }
}
