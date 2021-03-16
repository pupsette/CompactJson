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
            if (value >= 0)
                writer.Number((ulong)value);
            else
                writer.Number((long)value);
        }

        public override object FromNumber(long value)
        {
            if (value < int.MinValue || value > int.MaxValue)
                throw new System.Exception($"JSON number {value} cannot be converted to a 32 bit signed integer.");
            return unchecked((int)value);
        }

        public override object FromNumber(ulong value)
        {
            if (value > int.MaxValue)
                throw new System.Exception($"JSON number {value} cannot be converted to a 32 bit signed integer.");
            return unchecked((int)value);
        }
    }
}
