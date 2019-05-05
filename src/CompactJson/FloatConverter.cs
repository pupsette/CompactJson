namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class FloatConverter : NullableConverterBase<float>
    {
        public FloatConverter(bool nullable)
            : base(nullable)
        {
        }

        protected override void InternalWrite(float value, IJsonConsumer writer)
        {
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
    }
}
