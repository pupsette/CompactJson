namespace CompactJson
{
    /// <summary>
    /// A converter for strings.
    /// </summary>
    internal sealed class StringConverter : ConverterBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public StringConverter() : base(typeof(string))
        {
        }

        /// <summary>
        /// Converts JSON null value to a null string.
        /// </summary>
        /// <returns>null.</returns>
        public override object FromNull()
        {
            return null;
        }

        /// <summary>
        /// Writes the string or null value to the <see cref="IJsonConsumer"/>.
        /// </summary>
        /// <param name="value">The string value or null.</param>
        /// <param name="writer">The <see cref="IJsonConsumer"/></param>
        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
                writer.Null();
            else
                writer.String((string)value);
        }

        /// <summary>
        /// Converts the JSON string as-is.
        /// </summary>
        /// <param name="value">The JSON string, which is returned as result.</param>
        /// <returns>The string.</returns>
        public override object FromString(string value)
        {
            return value;
        }
    }
}
