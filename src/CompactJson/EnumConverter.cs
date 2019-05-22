using System;
using System.Collections.Generic;

namespace CompactJson
{
    internal sealed class EnumConverter : ConverterBase
    {
        private readonly Dictionary<string, object> mStringToEnumValue = new Dictionary<string, object>();
        private readonly Dictionary<object, string> mEnumValueToString = new Dictionary<object, string>();

        public EnumConverter(Type enumType)
            : base(enumType)
        {
            foreach (object enumValue in enumType.GetEnumValues())
            {
                mStringToEnumValue.Add(enumValue.ToString(), enumValue);
                mEnumValueToString.Add(enumValue, enumValue.ToString());
            }
        }

        public override void Write(object value, IJsonConsumer writer)
        {
            if (!mEnumValueToString.TryGetValue(value, out string str))
                throw new Exception($"Unrecognized enumeration value '{value}'. Valid values are {string.Join(", ", mStringToEnumValue.Keys)}.");
            writer.String(str);
        }

        public override object FromString(string value)
        {
            if (!mStringToEnumValue.TryGetValue(value, out object result))
                throw new Exception($"Unrecognized enumeration value '{value}'. Valid values are {string.Join(", ", mStringToEnumValue.Keys)}.");
            return result;
        }
    }
}
