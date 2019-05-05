using System;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    abstract class NullableConverterBase<T> : ConverterBase where T : struct
    {
        private readonly bool mNullable;

        protected NullableConverterBase(bool nullable)
            : base(nullable ? typeof(T?) : typeof(T))
        {
            mNullable = nullable;
        }

        public sealed override object FromNull()
        {
            if (mNullable)
                return null;

            return base.FromNull();
        }

        public sealed override void Write(object value, IJsonConsumer writer)
        {
            if (mNullable && value == null)
            {
                writer.Null();
                return;
            }

            InternalWrite((T)value, writer);
        }

        protected abstract void InternalWrite(T value, IJsonConsumer writer);
    }
}
