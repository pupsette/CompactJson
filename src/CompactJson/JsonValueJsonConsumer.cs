namespace CompactJson
{
    /// <summary>
    /// <see cref="IJsonConsumer"/> implementation which consumes
    /// JSON data from any producer and creates a JSON object model,
    /// <see cref="JsonValue"/> and derived classes.
    /// </summary>
    internal class JsonValueJsonConsumer : IJsonConsumer
    {
        public JsonValue Result;

        public void Boolean(bool value)
        {
            Result = new JsonBoolean(value);
        }

        public void Null()
        {
            Result = new JsonNull();
        }

        public void Number(double value)
        {
            Result = new JsonFloat(value);
        }

        public void Number(long value)
        {
            Result = new JsonLong(value);
        }

        public void String(string value)
        {
            Result = new JsonString(value);
        }

        public IJsonObjectConsumer Object()
        {
            JsonObject result = new JsonObject();
            Result = result;
            return result;
        }

        public IJsonArrayConsumer Array()
        {
            JsonArray result = new JsonArray();
            Result = result;
            return result;
        }
    }
}
