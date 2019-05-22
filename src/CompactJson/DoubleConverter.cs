namespace CompactJson
{
    internal sealed class DoubleConverter : NullableConverterBase<double>
    {
        public DoubleConverter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(double value, IJsonConsumer writer)
        {
            writer.Number(value);
        }

        public override object FromNumber(long value)
        {
            return (double)value;
        }

        public override object FromNumber(double value)
        {
            return value;
        }
    }
}
