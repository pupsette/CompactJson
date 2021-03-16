using System;
using System.Collections.Generic;

namespace CompactJson
{
    internal sealed class DictionaryConverter<T> : CollectionConverterBase
    {
        public DictionaryConverter(Func<IConverter> elementConverter)
            : base(typeof(Dictionary<string, T>), elementConverter)
        {
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
                ElementConverter.Write(kvp.Value, objectConsumer);
            }
            objectConsumer.Done();
        }

        public override IJsonObjectConsumer FromObject(Action<object> whenDone)
        {
            return new ObjectConsumer(ElementConverter, whenDone);
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
            public void Number(ulong value)
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
