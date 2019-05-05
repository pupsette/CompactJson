namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonNull : JsonValue
    {
        public override void Write(IJsonConsumer consumer)
        {
            consumer.Null();
        }
    }
}
