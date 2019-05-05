namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    abstract class JsonNumber : JsonValue
    {
        public abstract double AsDouble();
        public abstract long AsLong();
    }
}
