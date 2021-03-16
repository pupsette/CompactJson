namespace CompactJson
{
    /// <summary>
    /// An interface used as a sink for JSON compatible data.
    /// This interface is used to convert from one representation
    /// of the data to another.
    /// At the top-level and for other single values, there is only one
    /// method called.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IJsonConsumer
    {
        /// <summary>
        /// Start consuming an object.
        /// </summary>
        /// <returns>A consumer which is used to assign properties
        /// to the object.</returns>
        IJsonObjectConsumer Object();

        /// <summary>
        /// Start consuming an array.
        /// </summary>
        /// <returns>A consumer which is used to add items to
        /// the array.</returns>
        IJsonArrayConsumer Array();

        /// <summary>
        /// Consumes a null value.
        /// </summary>
        void Null();

        /// <summary>
        /// Consumes a boolean value.
        /// </summary>
        /// <param name="value">The bool value.</param>
        void Boolean(bool value);

        /// <summary>
        /// Consumes a double precision floating point value.
        /// </summary>
        /// <param name="value">The double value.</param>
        void Number(double value);

        /// <summary>
        /// Consumes a signed 64 bit integer value.
        /// </summary>
        /// <param name="value">The signed 64 bit integer value.</param>
        void Number(long value);

        /// <summary>
        /// Consumes an unsigned 64 bit integer value.
        /// </summary>
        /// <param name="value">The unsigned 64 bit integer value.</param>
        void Number(ulong value);

        /// <summary>
        /// Consumes a string value. The string is unescaped and
        /// does not include double-quotes.
        /// </summary>
        /// <param name="value">The string value.</param>
        void String(string value);
    }
}
