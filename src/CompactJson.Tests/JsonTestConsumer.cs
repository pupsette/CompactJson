using System.Globalization;
using System.Text;

namespace CompactJson.Tests
{
    public class JsonTestConsumer : IJsonConsumer
    {
        public class JsonObjectTestConsumer : JsonTestConsumer, IJsonObjectConsumer
        {
            public JsonObjectTestConsumer(JsonTestConsumer parent)
                : base(parent)
            {
            }

            public void Done()
            {
                mStringBuilder.Append("}");
            }

            public void PropertyName(string propertyName)
            {
                mStringBuilder.Append(propertyName);
                mStringBuilder.Append(":");
            }
        }

        public class JsonArrayTestConsumer : JsonTestConsumer, IJsonArrayConsumer
        {
            public JsonArrayTestConsumer(JsonTestConsumer parent)
                : base(parent)
            {
            }

            public void Done()
            {
                mStringBuilder.Append("]");
            }
        }

        private readonly StringBuilder mStringBuilder;

        public override string ToString()
        {
            return mStringBuilder.ToString();
        }

        public JsonTestConsumer()
        {
            mStringBuilder = new StringBuilder();
        }

        protected JsonTestConsumer(JsonTestConsumer parent)
        {
            this.mStringBuilder = parent.mStringBuilder;
        }

        public IJsonArrayConsumer Array()
        {
            mStringBuilder.Append("[");
            return new JsonArrayTestConsumer(this);
        }

        public void Boolean(bool value)
        {
            mStringBuilder.Append(value ? "T" : "F");
        }

        public void Null()
        {
            mStringBuilder.Append("N");
        }

        public void Number(double value)
        {
            mStringBuilder.Append("D" + value.ToString(CultureInfo.InvariantCulture));
        }

        public void Number(long value)
        {
            mStringBuilder.Append("L" + value.ToString(CultureInfo.InvariantCulture));
        }

        public IJsonObjectConsumer Object()
        {
            mStringBuilder.Append("{");
            return new JsonObjectTestConsumer(this);
        }

        public void String(string value)
        {
            mStringBuilder.Append(value);
        }
    }
}
