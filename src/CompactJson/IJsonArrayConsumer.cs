namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    interface IJsonArrayConsumer : IJsonConsumer
    {
        void Done();
    }
}
