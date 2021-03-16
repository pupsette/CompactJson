namespace CompactJson
{
    internal class JsonHashCodeBuilder : IJsonConsumer, IJsonArrayConsumer, IJsonObjectConsumer
    {
        private int cumulatedHashCode = 1;
        public int HashCode { get { return cumulatedHashCode; } }

        public IJsonArrayConsumer Array()
        {
            cumulatedHashCode *= 2;
            return this;
        }

        public void Boolean(bool value)
        {
            cumulatedHashCode = cumulatedHashCode * 7 + value.GetHashCode();
        }

        public void Done()
        {
            cumulatedHashCode += 11;
        }

        public void Null()
        {
            cumulatedHashCode *= 3;
        }

        public void Number(double value)
        {
            cumulatedHashCode = cumulatedHashCode * 5 + value.GetHashCode();
        }

        public void Number(long value)
        {
            cumulatedHashCode = cumulatedHashCode * 5 + value.GetHashCode();
        }

        public void Number(ulong value)
        {
            cumulatedHashCode = cumulatedHashCode * 5 + value.GetHashCode();
        }

        public IJsonObjectConsumer Object()
        {
            cumulatedHashCode += 19;
            return this;
        }

        public void PropertyName(string propertyName)
        {
            cumulatedHashCode = cumulatedHashCode * 12 + propertyName.GetHashCode();
        }

        public void String(string value)
        {
            cumulatedHashCode = cumulatedHashCode * 13 + value.GetHashCode();
        }
    }
}
