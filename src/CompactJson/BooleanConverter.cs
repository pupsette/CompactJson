namespace CompactJson
{
    internal sealed class BooleanConverter : NullableConverterBase<bool>
    {
        public BooleanConverter(bool nullable) : base(nullable)
        {
        }

        protected override void InternalWrite(bool value, IJsonConsumer writer)
        {
            writer.Boolean(value);
        }

        public override object FromBoolean(bool value)
        {
            return value;
        }
    }
}
