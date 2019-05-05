namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonLong : JsonNumber
    {
        public JsonLong(long value)
        {
            Value = value;
        }

        public long Value { get; }

        public override double AsDouble()
        {
            return (double)Value;
        }

        public override long AsLong()
        {
            return Value;
        }

        public override void Write(IJsonConsumer consumer)
        {
            consumer.Number(Value);
        }
    }
}
