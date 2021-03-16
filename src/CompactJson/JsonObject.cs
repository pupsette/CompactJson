using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompactJson
{
    /// <summary>
    /// This class represents a generic JSON object.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonObject : JsonValue, IReadOnlyDictionary<string, JsonValue>, IJsonObjectConsumer
    {
        private const int MAGIC_PROPERTY_COUNT_THRESHOLD = 5;
        private string mPropertyNameFromConsumer;
        private Dictionary<string, JsonValue> mIndexedProperties;
        private readonly List<KeyValuePair<string, JsonValue>> mPropertiesOrdered;
        private readonly Action<object> mWhenDone;

        /// <summary>
        /// Internal constructor for usage in a JSON consumer.
        /// </summary>
        /// <param name="whenDone">The callback to invoke when the
        /// JSON object is done.</param>
        internal JsonObject(Action<object> whenDone)
            : this()
        {
            this.mWhenDone = whenDone;
        }

        /// <summary>
        /// Initializes an empty JSON object.
        /// </summary>
        public JsonObject()
        {
            mPropertiesOrdered = new List<KeyValuePair<string, JsonValue>>();
        }

        /// <summary>
        /// Initializes a JSON object with the given properties. The properties
        /// are not validated. The property names and the values must not be null.
        /// Also the property names must not contain duplicates.
        /// </summary>
        public JsonObject(IEnumerable<KeyValuePair<string, JsonValue>> properties)
        {
            mPropertiesOrdered = new List<KeyValuePair<string, JsonValue>>(properties);
        }

        /// <summary>
        /// Returns the value of the property with the given <paramref name="propertyName"/>.
        /// If the property is not present, an exception is thrown.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        /// <seealso cref="TryGetProperty(string, out JsonValue)"/>
        public JsonValue this[string propertyName]
        {
            get
            {
                if (!TryGetProperty(propertyName, out JsonValue result))
                    throw new KeyNotFoundException($"Property '{propertyName}' could not be found.");
                return result;
            }
        }

        /// <summary>
        /// Tries to return the value of the property with the given <paramref name="propertyName"/>.
        /// If the property is not present, false is returned.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="jsonValue">The value, if the property is present; null, otherwise.</param>
        /// <returns>true, if the property is present; false, otherwise.</returns>
        public bool TryGetProperty(string propertyName, out JsonValue jsonValue)
        {
            if (mPropertiesOrdered.Count < MAGIC_PROPERTY_COUNT_THRESHOLD)
            {
                for (int i = 0; i < mPropertiesOrdered.Count; i++)
                {
                    if (mPropertiesOrdered[i].Key == propertyName)
                    {
                        jsonValue = mPropertiesOrdered[i].Value;
                        return true;
                    }
                }
                jsonValue = null;
                return false;
            }
            else
            {
                if (mIndexedProperties == null)
                {
                    mIndexedProperties = new Dictionary<string, JsonValue>();
                    for (int i = 0; i < mPropertiesOrdered.Count; i++)
                        mIndexedProperties.Add(mPropertiesOrdered[i].Key, mPropertiesOrdered[i].Value);
                }

                return mIndexedProperties.TryGetValue(propertyName, out jsonValue);
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, JsonValue>.Keys
        {
            get { return mPropertiesOrdered.Select(kvp => kvp.Key); }
        }

        IEnumerable<JsonValue> IReadOnlyDictionary<string, JsonValue>.Values
        {
            get { return mPropertiesOrdered.Select(kvp => kvp.Value); }
        }

        /// <summary>
        /// Gets the number of properties of this JSON object.
        /// </summary>
        public int PropertyCount
        {
            get { return mPropertiesOrdered.Count; }
        }

        int IReadOnlyCollection<KeyValuePair<string, JsonValue>>.Count
        {
            get { return mPropertiesOrdered.Count; }
        }

        /// <summary>
        /// This method checks, if a property with the given <paramref name="propertyName"/>
        /// is present or not.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>true, if the property exists; false, otherwise.</returns>
        public bool HasProperty(string propertyName)
        {
            return TryGetProperty(propertyName, out _);
        }

        bool IReadOnlyDictionary<string, JsonValue>.ContainsKey(string key)
        {
            return TryGetProperty(key, out _ );
        }

        IEnumerator<KeyValuePair<string, JsonValue>> IEnumerable<KeyValuePair<string, JsonValue>>.GetEnumerator()
        {
            return mPropertiesOrdered.GetEnumerator();
        }

        bool IReadOnlyDictionary<string, JsonValue>.TryGetValue(string key, out JsonValue value)
        {
            return TryGetProperty(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mPropertiesOrdered.GetEnumerator();
        }

        void IJsonObjectConsumer.Done()
        {
            mWhenDone?.Invoke(this);
        }

        /// <summary>
        /// Writes this <see cref="JsonObject"/> to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="JsonValue.ToModel(System.Type)"/> and 
        /// <see cref="JsonValue.ToModel{T}"/> in order to convert this generic object 
        /// model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            IJsonObjectConsumer obj = consumer.Object();
            foreach (KeyValuePair<string, JsonValue> property in mPropertiesOrdered)
            {
                obj.PropertyName(property.Key);
                property.Value.Write(obj);
            }
            obj.Done();
        }

        void IJsonObjectConsumer.PropertyName(string propertyName)
        {
            mPropertyNameFromConsumer = propertyName;
            mIndexedProperties = null;
        }

        IJsonObjectConsumer IJsonConsumer.Object()
        {
            var result = new JsonObject();
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, result));
            return result;
        }

        IJsonArrayConsumer IJsonConsumer.Array()
        {
            var result = new JsonArray();
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, result));
            return result;
        }

        void IJsonConsumer.Null()
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonNull()));
        }

        void IJsonConsumer.Boolean(bool value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonBoolean(value)));
        }

        void IJsonConsumer.Number(double value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonNumber(value)));
        }

        void IJsonConsumer.Number(long value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonNumber(value)));
        }
        void IJsonConsumer.Number(ulong value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonNumber(value)));
        }

        void IJsonConsumer.String(string value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonString(value)));
        }
    }
}
