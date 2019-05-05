namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    interface IJsonObjectConsumer : IJsonConsumer
    {
        void PropertyName(string propertyName);
        void Done();
    }
}
