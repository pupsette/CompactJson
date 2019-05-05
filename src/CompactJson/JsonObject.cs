using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonObject : JsonValue, IReadOnlyDictionary<string, JsonValue>, IJsonObjectConsumer
    {
        private string mPropertyNameFromConsumer;
        private Dictionary<string, JsonValue> mIndexedProperties;
        private readonly List<KeyValuePair<string, JsonValue>> mPropertiesOrdered;
        private readonly Action<object> mWhenDone;

        internal JsonObject(Action<object> whenDone)
            : this()
        {
            this.mWhenDone = whenDone;
        }

        public JsonObject()
        {
            mPropertiesOrdered = new List<KeyValuePair<string, JsonValue>>();
        }

        public JsonValue this[string key]
        {
            get
            {
                if (!TryGetProperty(key, out JsonValue result))
                    throw new KeyNotFoundException($"Property '{key}' could not be found.");
                return result;
            }
        }

        public bool TryGetProperty(string propertyName, out JsonValue jsonValue)
        {
            if (mPropertiesOrdered.Count < 5)
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

        public IEnumerable<string> Keys
        {
            get { return mPropertiesOrdered.Select(kvp => kvp.Key); }
        }

        public IEnumerable<JsonValue> Values
        {
            get { return mPropertiesOrdered.Select(kvp => kvp.Value); }
        }

        public int Count
        {
            get { return mPropertiesOrdered.Count; }
        }

        public bool ContainsKey(string key)
        {
            return TryGetProperty(key, out _ );
        }

        public IEnumerator<KeyValuePair<string, JsonValue>> GetEnumerator()
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
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonFloat(value)));
        }

        void IJsonConsumer.Number(long value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonLong(value)));
        }

        void IJsonConsumer.String(string value)
        {
            mPropertiesOrdered.Add(new KeyValuePair<string, JsonValue>(mPropertyNameFromConsumer, new JsonString(value)));
        }
    }
}
