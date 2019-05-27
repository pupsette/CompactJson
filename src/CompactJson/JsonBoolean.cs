namespace CompactJson
{
    /// <summary>
    /// This class represents a JSON boolean value.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonBoolean : JsonValue
    {
        /// <summary>
        /// Initializes a new JSON boolean value with the given value.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public JsonBoolean(bool value)
        {
            Value = value;
        }

        /// <summary>
        /// The boolean value of this JSON boolean.
        /// </summary>
        public bool Value { get; }

        /// <summary>
        /// Writes this <see cref="JsonBoolean"/> to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="JsonValue.ToModel(System.Type)"/> and 
        /// <see cref="JsonValue.ToModel{T}"/> in order to convert this generic object 
        /// model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            consumer.Boolean(Value);
        }
    }
}
