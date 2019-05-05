using System;

namespace CompactJson
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
#if COMPACTJSON_PUBLIC
    public
#endif
    class MultiTypePropertyAttribute : Attribute
    {
        public MultiTypePropertyAttribute(string propertyName, Type objectType)
        {
            PropertyName = propertyName;
            ObjectType = objectType;
        }

        public string PropertyName { get; }

        public Type ObjectType { get; }
    }
}
