namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IJsonArrayConsumer : IJsonConsumer
    {
        void Done();
    }
}
