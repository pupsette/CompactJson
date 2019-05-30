using System;

namespace CompactJson
{
    internal sealed class GuidConverter : NullableConverterBase<Guid>
    {
        public GuidConverter(bool nullable) : base(nullable)
        {
        }

        protected override void InternalWrite(Guid value, IJsonConsumer writer)
        {
            writer.String(value.ToString());
        }

        public override object FromString(string value)
        {
            return Guid.Parse(value);
        }
    }
}
