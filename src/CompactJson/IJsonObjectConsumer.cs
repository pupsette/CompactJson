namespace CompactJson
{
    /// <summary>
    /// An interface used for writing JSON objects.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IJsonObjectConsumer : IJsonConsumer
    {
        /// <summary>
        /// Sets the property name for the next call to one of
        /// the other methods of <see cref="IJsonConsumer"/>.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        void PropertyName(string propertyName);

        /// <summary>
        /// Is called when the object is done and no more
        /// properties will be supplied.
        /// </summary>
        void Done();
    }
}
