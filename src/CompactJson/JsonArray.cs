using System;
using System.Collections;
using System.Collections.Generic;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonArray : JsonValue, IReadOnlyList<JsonValue>, IJsonArrayConsumer
    {
        internal JsonArray(Action<object> whenDone)
            : this()
        {
            this.mWhenDone = whenDone;
        }

        public JsonArray()
        {
            mList = new List<JsonValue>();
        }

        public JsonArray(IList<JsonValue> data)
        {
            mList = data;
        }

        private readonly IList<JsonValue> mList;
        private readonly Action<object> mWhenDone;

        public JsonValue this[int index]
        {
            get { return mList[index]; }
        }

        public int Count { get { return mList.Count; } }

        IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator()
        {
            return ((IEnumerable<JsonValue>)mList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IJsonObjectConsumer IJsonConsumer.Object()
        {
            JsonObject result = new JsonObject();
            mList.Add(result);
            return result;
        }

        IJsonArrayConsumer IJsonConsumer.Array()
        {
            JsonArray result = new JsonArray();
            mList.Add(result);
            return result;
        }

        void IJsonConsumer.Null()
        {
            mList.Add(new JsonNull());
        }

        void IJsonConsumer.Boolean(bool value)
        {
            mList.Add(new JsonBoolean(value));
        }

        void IJsonConsumer.Number(double value)
        {
            mList.Add(new JsonFloat(value));
        }

        void IJsonConsumer.Number(long value)
        {
            mList.Add(new JsonLong(value));
        }

        void IJsonConsumer.String(string value)
        {
            mList.Add(new JsonString(value));
        }

        void IJsonArrayConsumer.Done()
        {
            mWhenDone?.Invoke(this);
        }

        public override void Write(IJsonConsumer consumer)
        {
            IJsonArrayConsumer arrayConsumer = consumer.Array();
            for (int i = 0; i < mList.Count; i++)
                mList[i].Write(arrayConsumer);
            arrayConsumer.Done();
        }
    }
}
