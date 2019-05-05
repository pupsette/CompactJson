namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class StringConverter : ConverterBase
    {
        public StringConverter() : base(typeof(string))
        {
        }

        public override object FromNull()
        {
            return null;
        }

        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
                writer.Null();
            else
                writer.String((string)value);
        }

        public override object FromString(string value)
        {
            return value;
        }
    }
}
