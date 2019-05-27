namespace CompactJson
{
    /// <summary>
    /// This class represents a JSON number.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    abstract class JsonNumber : JsonValue
    {
        /// <summary>
        /// Returns the JSON number as floating point value.
        /// </summary>
        /// <returns>The floating point value.</returns>
        public abstract double AsDouble();

        /// <summary>
        /// Returns the JSON number as 64 bit signed integer value.
        /// </summary>
        /// <returns>The integer value.</returns>
        public abstract long AsLong();
    }
}
