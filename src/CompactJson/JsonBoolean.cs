namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonBoolean : JsonValue
    {
        public JsonBoolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override void Write(IJsonConsumer consumer)
        {
            consumer.Boolean(Value);
        }
    }
}
