namespace CompactJson
{
    /// <summary>
    /// An interface used as a sink for JSON compatible arrays.
    /// This interface is used to convert from one representation
    /// of the data to another.
    /// 
    /// The methods inherited from <see cref="IJsonConsumer"/>
    /// will write the array elements. If the array has no more
    /// elements, the producer will call <see cref="Done"/>.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IJsonArrayConsumer : IJsonConsumer
    {
        /// <summary>
        /// Called by the producer to signal, that the array has
        /// no more elements.
        /// </summary>
        void Done();
    }
}
