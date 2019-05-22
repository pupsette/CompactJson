using System;

namespace CompactJson
{
    internal sealed class UnspecificConverter : JsonValueConverter
    {
        public UnspecificConverter()
            : base(typeof(object), allowJsonArray: true, allowJsonObject: true)
        {
        }

        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
                writer.Null();
            else
            {
                Type type = value.GetType();
                if (type == typeof(object)) // prevent recursion
                    writer.Object().Done();
                else
                {
                    IConverter converter = ConverterRegistry.Get(type);
                    converter.Write(value, writer);
                }
            }
        }

        public override object FromNull()
        {
            return null;
        }

        public override object FromBoolean(bool value)
        {
            return value;
        }

        public override object FromNumber(double value)
        {
            return value;
        }

        public override object FromNumber(long value)
        {
            return value;
        }

        public override object FromString(string value)
        {
            return value;
        }
    }
}
