namespace CompactJson
{
    internal sealed class Int64Converter : NullableConverterBase<long>
    {
        public Int64Converter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(long value, IJsonConsumer writer)
        {
            writer.Number(value);
        }

        public override object FromNumber(long value)
        {
            return value;
        }
    }
}
