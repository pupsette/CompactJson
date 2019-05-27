namespace CompactJson
{
    /// <summary>
    /// This class represents the JSON null value.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonNull : JsonValue
    {
        /// <summary>
        /// Writes null to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="JsonValue.ToModel(System.Type)"/> and 
        /// <see cref="JsonValue.ToModel{T}"/> in order to convert this generic object 
        /// model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            consumer.Null();
        }
    }
}
