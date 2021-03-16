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
            if (value >= 0)
                writer.Number((ulong)value);
            else
                writer.Number(value);
        }

        public override object FromNumber(long value)
        {
            return value;
        }

        public override object FromNumber(ulong value)
        {
            if (value > long.MaxValue)
                throw new System.Exception($"JSON number {value} cannot be converted to a 64 bit signed integer.");
            return unchecked((long)value);
        }
    }
}
