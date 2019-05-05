namespace CompactJson
{
    /// <summary>
    /// A converter for converting byte arrays to base64 encoded
    /// strings and back.
    /// </summary>
    internal sealed class ByteArrayConverter : ConverterBase
    {
        public ByteArrayConverter() : base(typeof(byte[]))
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
                writer.String(System.Convert.ToBase64String((byte[])value, System.Base64FormattingOptions.None));
        }

        public override object FromString(string value)
        {
            if (value == null)
                return null;
            else
                return System.Convert.FromBase64String(value);
        }
    }
}
