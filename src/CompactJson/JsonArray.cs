using System;
using System.Collections;
using System.Collections.Generic;

namespace CompactJson
{
    /// <summary>
    /// The class represents a generic JSON array.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonArray : JsonValue, IReadOnlyList<JsonValue>, IJsonArrayConsumer
    {
        /// <summary>
        /// Internal constructor for usage in a JSON consumer.
        /// </summary>
        /// <param name="whenDone">The callback to invoke when the
        /// array is done.</param>
        internal JsonArray(Action<object> whenDone)
            : this()
        {
            this.mWhenDone = whenDone;
        }

        /// <summary>
        /// Initializes an empty array.
        /// </summary>
        public JsonArray()
        {
            mList = new List<JsonValue>();
        }

        /// <summary>
        /// Initializes the JSON array with the given list.
        /// The list is not copied, but a reference to it is held.
        /// Hence, adding/removing items from the JSON array will modify
        /// the originally passed list.
        /// </summary>
        public JsonArray(IList<JsonValue> data)
        {
            mList = data;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public JsonValue this[int index]
        {
            get { return mList[index]; }
            set { mList[index] = value; }
        }

        /// <summary>
        /// Gets the number of elements contained in this JSON array.
        /// </summary>
        public int Count { get { return mList.Count; } }

        IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator()
        {
            return mList.GetEnumerator();
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
            mList.Add(new JsonNumber(value));
        }

        void IJsonConsumer.Number(long value)
        {
            mList.Add(new JsonNumber(value));
        }

        void IJsonConsumer.Number(ulong value)
        {
            mList.Add(new JsonNumber(value));
        }

        void IJsonConsumer.String(string value)
        {
            mList.Add(new JsonString(value));
        }

        void IJsonArrayConsumer.Done()
        {
            mWhenDone?.Invoke(this);
        }

        /// <summary>
        /// Writes this <see cref="JsonArray"/> to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="JsonValue.ToModel(System.Type)"/> and 
        /// <see cref="JsonValue.ToModel{T}"/> in order to convert this generic object 
        /// model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            IJsonArrayConsumer arrayConsumer = consumer.Array();
            for (int i = 0; i < mList.Count; i++)
                mList[i].Write(arrayConsumer);
            arrayConsumer.Done();
        }

        private readonly IList<JsonValue> mList;
        private readonly Action<object> mWhenDone;
    }
}
