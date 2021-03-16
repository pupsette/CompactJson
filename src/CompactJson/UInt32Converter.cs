namespace CompactJson
{
    internal sealed class UInt32Converter : NullableConverterBase<uint>
    {
        public UInt32Converter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(uint value, IJsonConsumer writer)
        {
            writer.Number((ulong)value);
        }

        public override object FromNumber(long value)
        {
            if (value < 0 || value > uint.MaxValue)
                throw new System.Exception($"JSON number {value} cannot be converted to a 32 bit unsigned integer.");
            return unchecked((uint)value);
        }

        public override object FromNumber(ulong value)
        {
            if (value > uint.MaxValue)
                throw new System.Exception($"JSON number {value} cannot be converted to a 32 bit unsigned integer.");
            return unchecked((uint)value);
        }
    }
}
