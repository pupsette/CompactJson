namespace CompactJson
{
    internal sealed class DoubleConverter : NullableConverterBase<double>
    {
        internal const string NAN = "NaN";
        internal const string POS_INFINITY = "Infinity";
        internal const string POS_INFINITY_ALT = "+Infinity";
        internal const string NEG_INFINITY = "-Infinity";

        public DoubleConverter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(double value, IJsonConsumer writer)
        {
            if (double.IsNaN(value))
                writer.String(NAN);
            else if (double.IsPositiveInfinity(value))
                writer.String(POS_INFINITY);
            else if (double.IsNegativeInfinity(value))
                writer.String(NEG_INFINITY);
            else
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

        public override object FromNumber(ulong value)
        {
            return (double)value;
        }

        public override object FromString(string value)
        {
            if (value.Equals(NAN, System.StringComparison.OrdinalIgnoreCase))
                return double.NaN;
            if (value.Equals(POS_INFINITY, System.StringComparison.OrdinalIgnoreCase) || value.Equals(POS_INFINITY_ALT, System.StringComparison.OrdinalIgnoreCase))
                return double.PositiveInfinity;
            if (value.Equals(NEG_INFINITY, System.StringComparison.OrdinalIgnoreCase))
                return double.NegativeInfinity;

            return base.FromString(value);
        }
    }
}
