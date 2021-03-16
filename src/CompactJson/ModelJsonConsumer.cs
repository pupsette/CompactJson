namespace CompactJson
{
    internal class ModelJsonConsumer : IJsonConsumer
    {
        private readonly IConverter mConverter;

        public object Result { get; private set; }

        public ModelJsonConsumer(IConverter converter)
        {
            this.mConverter = converter;
        }

        public void Boolean(bool value)
        {
            Result = mConverter.FromBoolean(value);
        }

        public void Null()
        {
            Result = mConverter.FromNull();
        }

        public void Number(double value)
        {
            Result = mConverter.FromNumber(value);
        }

        public void Number(long value)
        {
            Result = mConverter.FromNumber(value);
        }

        public void Number(ulong value)
        {
            Result = mConverter.FromNumber(value);
        }

        public void String(string value)
        {
            Result = mConverter.FromString(value);
        }

        public IJsonObjectConsumer Object()
        {
            return mConverter.FromObject(SetResult);
        }

        public IJsonArrayConsumer Array()
        {
            return mConverter.FromArray(SetResult);
        }

        private void SetResult(object value)
        {
            Result = value;
        }
    }
}
