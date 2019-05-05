using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class ListConverter : ConverterBase
    {
        private readonly Func<Action<object>, IJsonArrayConsumer> mConsumerFactory;
        private readonly IConverter mItemConverter;

        public ListConverter(Type type, IConverter itemConverter, bool outputArray) : base(type)
        {
            this.mItemConverter = itemConverter;

            Type consumerType = typeof(Consumer<>).MakeGenericType(itemConverter.Type);
            System.Reflection.ConstructorInfo consumerConstructor = consumerType.GetConstructor(new[] { typeof(IConverter), typeof(bool), typeof(Action<object>) });

            ParameterExpression whenDoneParameter = Expression.Parameter(typeof(Action<object>), "whenDone");
            ConstantExpression converterParameter = Expression.Constant(itemConverter, typeof(IConverter));
            ConstantExpression outputArrayParameter = Expression.Constant(outputArray, typeof(bool));
            Expression newExpression = Expression.New(consumerConstructor, converterParameter, outputArrayParameter, whenDoneParameter);

            this.mConsumerFactory = Expression.Lambda<Func<Action<object>, IJsonArrayConsumer>>(newExpression, whenDoneParameter).Compile();
        }

        public override object FromNull()
        {
            return null;
        }

        public override IJsonArrayConsumer FromArray(Action<object> whenDone)
        {
            return mConsumerFactory(whenDone);
        }

        private class Consumer<T> : IJsonArrayConsumer
        {
            private readonly IConverter mElementConverter;
            private readonly bool mOutputArray;
            private readonly List<T> mList;
            private readonly Action<object> mWhenDone;

            public Consumer(IConverter elementConverter, bool outputArray, Action<object> whenDone)
            {
                Debug.Assert(elementConverter.Type == typeof(T));
                this.mElementConverter = elementConverter;
                this.mOutputArray = outputArray;
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
                if (mOutputArray)
                    mWhenDone(mList.ToArray());
                else
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

            public IJsonObjectConsumer Object()
            {
                return mElementConverter.FromObject(value => mList.Add((T)value));
            }

            public void String(string value)
            {
                mList.Add((T)mElementConverter.FromString(value));
            }
        }

        public override void Write(object value, IJsonConsumer writer)
        {
            var arrayConsumer = writer.Array();
            IList list = (IList)value; // Array and List<> implement IList
            for (int i = 0; i < list.Count; i++)
                mItemConverter.Write(list[i], arrayConsumer);
            arrayConsumer.Done();
        }
    }
}
