using System;

namespace CompactJson
{
    /// <summary>
    /// A converter factory for nullable types. This adds support for all
    /// 'Nullable&lt;&gt;' types.
    /// </summary>
    internal sealed class NullableConverterFactory : IConverterFactory
    {
        public IConverter Create(Type type, object[] parameters)
        {
            return new Converter(type);
        }

        public bool CanConvert(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return true;

            return false;
        }

        private class Converter : ConverterBase
        {
            private readonly IConverter valueTypeConverter;

            public Converter(Type nullableType)
                : base(nullableType)
            {
                valueTypeConverter = ConverterRegistry.Get(nullableType.GenericTypeArguments[0]);
            }

            public override object FromNull()
            {
                return null;
            }

            public override IJsonArrayConsumer FromArray(Action<object> whenDone)
            {
                return valueTypeConverter.FromArray(whenDone);
            }

            public override object FromBoolean(bool value)
            {
                return valueTypeConverter.FromBoolean(value);
            }

            public override object FromNumber(double value)
            {
                return valueTypeConverter.FromNumber(value);
            }

            public override object FromNumber(long value)
            {
                return valueTypeConverter.FromNumber(value);
            }

            public override object FromNumber(ulong value)
            {
                return valueTypeConverter.FromNumber(value);
            }

            public override IJsonObjectConsumer FromObject(Action<object> whenDone)
            {
                return valueTypeConverter.FromObject(whenDone);
            }

            public override object FromString(string value)
            {
                return valueTypeConverter.FromString(value);
            }

            public override void Write(object value, IJsonConsumer writer)
            {
                if (value == null)
                {
                    writer.Null();
                    return;
                }

                valueTypeConverter.Write(value, writer);
            }
        }
    }
}