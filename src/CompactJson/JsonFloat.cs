namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonFloat : JsonNumber
    {
        public JsonFloat(double value)
        {
            Value = value;
        }

        public double Value { get; }

        public override double AsDouble()
        {
            return Value;
        }

        public override long AsLong()
        {
            return (long)Value;
        }

        public override void Write(IJsonConsumer consumer)
        {
            consumer.Number(Value);
        }
    }
}
