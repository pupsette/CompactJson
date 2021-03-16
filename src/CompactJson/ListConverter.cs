using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CompactJson
{
    internal sealed class ListConverter<T> : CollectionConverterBase
    {
        public ListConverter(IConverter elementConverter) : base(typeof(List<T>), () => elementConverter)
        {
        }

        public override object FromNull()
        {
            return null;
        }

        public override IJsonArrayConsumer FromArray(Action<object> whenDone)
        {
            return new Consumer(ElementConverter, whenDone);
        }

        #region NESTED CONSUMER CLASS

        internal class Consumer : IJsonArrayConsumer
        {
            private readonly IConverter mElementConverter;
            private readonly List<T> mList;
            private readonly Action<List<T>> mWhenDone;

            public Consumer(IConverter elementConverter, Action<List<T>> whenDone)
            {
                Debug.Assert(elementConverter.Type == typeof(T));
                this.mElementConverter = elementConverter;
                this.mList = new List<T>();
                this.mWhenDone = whenDone;
            }

            public IJsonArrayConsumer Array()
            {
                return mElementConverter.FromArray(value => mList.Add((T)value));
            }

            public void Boolean(bool value)
            {
                mList.Add((T)mElementConverter.FromBoolean(value));
            }

            public void Done()
            {
                mWhenDone(mList);
            }

            public void Null()
            {
                mList.Add((T)mElementConverter.FromNull());
            }

            public void Number(double value)
            {
                mList.Add((T)mElementConverter.FromNumber(value));
            }

            public void Number(long value)
            {
                mList.Add((T)mElementConverter.FromNumber(value));
            }

            public void Number(ulong value)
            {
                mList.Add((T)mElementConverter.FromNumber(value));
            }

            public IJsonObjectConsumer Object()
            {
                return mElementConverter.FromObject(value => mList.Add((T)value));
            }

            public void String(string value)
            {
                mList.Add((T)mElementConverter.FromString(value));
            }
        }

        #endregion
    }
}
