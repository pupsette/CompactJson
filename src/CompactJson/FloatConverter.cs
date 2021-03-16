namespace CompactJson
{
    internal sealed class FloatConverter : NullableConverterBase<float>
    {
        public FloatConverter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(float value, IJsonConsumer writer)
        {
            if (float.IsNaN(value))
                writer.String(DoubleConverter.NAN);
            else if (float.IsPositiveInfinity(value))
                writer.String(DoubleConverter.POS_INFINITY);
            else if (float.IsNegativeInfinity(value))
                writer.String(DoubleConverter.NEG_INFINITY);
            else
                writer.Number((double)value);
        }

        public override object FromNumber(long value)
        {
            return (float)value;
        }

        public override object FromNumber(double value)
        {
            return (float)value;
        }

        public override object FromNumber(ulong value)
        {
            return (float)value;
        }

        public override object FromString(string value)
        {
            if (value.Equals(DoubleConverter.NAN, System.StringComparison.OrdinalIgnoreCase))
                return float.NaN;
            if (value.Equals(DoubleConverter.POS_INFINITY, System.StringComparison.OrdinalIgnoreCase) || value.Equals(DoubleConverter.POS_INFINITY_ALT, System.StringComparison.OrdinalIgnoreCase))
                return float.PositiveInfinity;
            if (value.Equals(DoubleConverter.NEG_INFINITY, System.StringComparison.OrdinalIgnoreCase))
                return float.NegativeInfinity;

            return base.FromString(value);
        }
    }
}
