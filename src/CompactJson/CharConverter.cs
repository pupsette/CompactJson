namespace CompactJson
{
    internal sealed class CharConverter : NullableConverterBase<char>
    {
        public CharConverter(bool nullable) : base(nullable)
        {
        }

        protected override void InternalWrite(char value, IJsonConsumer writer)
        {
            writer.String(new string(value, 1));
        }

        public override object FromString(string value)
        {
            if (value.Length != 1)
                throw new System.Exception($"Unable to convert string '{value}' to a single character.");
            return value[0];
        }
    }
}
