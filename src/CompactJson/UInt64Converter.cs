namespace CompactJson
{
    internal sealed class UInt64Converter : NullableConverterBase<ulong>
    {
        public UInt64Converter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(ulong value, IJsonConsumer writer)
        {
            writer.Number(value);
        }

        public override object FromNumber(long value)
        {
            if (value < 0)
                throw new System.Exception($"JSON number {value} cannot be converted to a 64 bit unsigned integer.");
            return unchecked((ulong)value);
        }

        public override object FromNumber(ulong value)
        {
            return value;
        }
    }
}
