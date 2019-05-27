namespace CompactJson
{
    /// <summary>
    /// The class represents a JSON floating point value (JSON number).
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonFloat : JsonNumber
    {
        /// <summary>
        /// Initializes a new JSON floating point with the given value.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        public JsonFloat(double value)
        {
            Value = value;
        }

        /// <summary>
        /// The floating point value represented by this JSON number.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Returns the JSON number as floating point value.
        /// </summary>
        /// <returns>The floating point value.</returns>
        public override double AsDouble()
        {
            return Value;
        }

        /// <summary>
        /// Returns the JSON number as 64 bit signed integer value.
        /// </summary>
        /// <returns>The integer value.</returns>
        public override long AsLong()
        {
            return (long)Value;
        }

        /// <summary>
        /// Writes this <see cref="JsonFloat"/> to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="JsonValue.ToModel(System.Type)"/> and 
        /// <see cref="JsonValue.ToModel{T}"/> in order to convert this generic object 
        /// model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            consumer.Number(Value);
        }
    }
}
