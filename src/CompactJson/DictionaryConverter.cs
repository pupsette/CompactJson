using System;
using System.Collections.Generic;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class DictionaryConverter : IConverterFactory
    {
        public IConverter Create(Type type)
        {
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                throw new Exception($"{nameof(DictionaryConverter)} cannot convert values of type '{type}'. It must be a 'System.Collections.Generic.Dictionary'.");

            Type[] genericTypes = type.GetGenericArguments();
            if (genericTypes[0] != typeof(string))
                throw new Exception($"{nameof(DictionaryConverter)} cannot convert values of type '{type}'. The keys must be strings.");

            Type specializedConverterType = typeof(SpecializedDictionaryConverter<>).MakeGenericType(genericTypes[1]);
            return (ConverterBase)Activator.CreateInstance(specializedConverterType, new object[] { GetValueConverter(genericTypes[1]) });
        }

        /// <summary>
        /// Virtual method for retrieving an instance of <see cref="IConverter"/> for
        /// converting the values of the dictionary. The default implementation uses the
        /// <see cref="ConverterRegistry"/> to obtain a type-specific converter. You may
        /// override this in deriving classes in order to change the behavior.
        /// </summary>
        /// <param name="dictionaryValueType">The type of the dictionary values.</param>
        /// <returns>An instance of <see cref="IConverter"/> for converting
        /// the values of the dictionary.</returns>
        protected virtual IConverter GetValueConverter(Type dictionaryValueType)
        {
            return ConverterRegistry.Get(dictionaryValueType);
        }

        private class SpecializedDictionaryConverter<T> : ConverterBase
        {
            private readonly IConverter valueConverter;

            public SpecializedDictionaryConverter(IConverter valueConverter)
                : base(typeof(Dictionary<string, T>))
            {
                this.valueConverter = valueConverter;
            }

            public override void Write(object value, IJsonConsumer writer)
            {
                if (value == null)
                {
                    writer.Null();
                    return;
                }

                IJsonObjectConsumer objectConsumer = writer.Object();
                foreach (KeyValuePair<string, T> kvp in (Dictionary<string, T>)value)
                {
                    objectConsumer.PropertyName(kvp.Key);
                    valueConverter.Write(kvp.Value, objectConsumer);
                }
                objectConsumer.Done();
            }

            public override IJsonObjectConsumer FromObject(Action<object> whenDone)
            {
                return new ObjectConsumer(valueConverter, whenDone);
            }

            public override object FromNull()
            {
                return null;
            }

            private class ObjectConsumer : IJsonObjectConsumer
            {
                private readonly Dictionary<string, T> mResult;
                private string mKey;
                private readonly IConverter mValueConverter;
                private readonly Action<object> mWhenDone;

                public ObjectConsumer(IConverter valueConverter, Action<object> whenDone)
                {
                    this.mValueConverter = valueConverter;
                    this.mWhenDone = whenDone;
                    this.mResult = new Dictionary<string, T>();
                }

                public IJsonArrayConsumer Array()
                {
                    return mValueConverter.FromArray(obj => mResult.Add(mKey, (T)obj));
                }

                public void Boolean(bool value)
                {
                    mResult.Add(mKey, (T)mValueConverter.FromBoolean(value));
                }

                public void Done()
                {
                    mWhenDone(mResult);
                }

                public void Null()
                {
                    mResult.Add(mKey, (T)mValueConverter.FromNull());
                }

                public void Number(double value)
                {
                    mResult.Add(mKey, (T)mValueConverter.FromNumber(value));
                }

                public void Number(long value)
                {
                    mResult.Add(mKey, (T)mValueConverter.FromNumber(value));
                }

                public IJsonObjectConsumer Object()
                {
                    return mValueConverter.FromObject(obj => mResult.Add(mKey, (T)obj));
                }

                public void PropertyName(string propertyName)
                {
                    mKey = propertyName;
                }

                public void String(string value)
                {
                    mResult.Add(mKey, (T)mValueConverter.FromString(value));
                }
            }
        }
    }
}
