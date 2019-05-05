using System;

namespace CompactJson
{
    /// <summary>
    /// Base class for JSON to/from .NET object conversion.
    /// By default all JSON tokens will be refused. Deriving implementations
    /// should override the methods of the accepted JSON token types (e.g. strings).
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    abstract class ConverterBase : IConverter
    {
        protected ConverterBase(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public virtual IJsonArrayConsumer FromArray(Action<object> whenDone)
        {
            throw new Exception($"Cannot convert JSON array to type '{Type}'.");
        }

        public virtual object FromBoolean(bool value)
        {
            throw new Exception($"Cannot convert JSON boolean to type '{Type}'.");
        }

        public virtual object FromNull()
        {
            throw new Exception($"Cannot convert JSON null to type '{Type}'.");
        }

        public virtual object FromNumber(double value)
        {
            throw new Exception($"Cannot convert JSON floating point number to type '{Type}'.");
        }

        public virtual object FromNumber(long value)
        {
            throw new Exception($"Cannot convert JSON integer number to type '{Type}'.");
        }

        public virtual IJsonObjectConsumer FromObject(Action<object> whenDone)
        {
            throw new Exception($"Cannot convert JSON object to type '{Type}'.");
        }

        public virtual object FromString(string value)
        {
            throw new Exception($"Cannot convert JSON string to type '{Type}'.");
        }

        /// <summary>
        /// This method converts the given value to JSON by invoking
        /// appropriate methods of the <see cref="IJsonConsumer"/>.
        /// </summary>
        /// <param name="value">The value to write. Implementors may assume
        /// that the type of the incoming value can be cast to the target type 
        /// of this converter, however, it may be null.</param>
        /// <param name="writer">The consumer which must be used for writing 
        /// JSON data according to the contents of the given value.</param>
        public abstract void Write(object value, IJsonConsumer writer);
    }
}
