namespace CompactJson
{
    internal sealed class Int32Converter : NullableConverterBase<int>
    {
        public Int32Converter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(int value, IJsonConsumer writer)
        {
            writer.Number((long)value);
        }

        public override object FromNumber(long value)
        {
            return (int)value;
        }
    }
}
