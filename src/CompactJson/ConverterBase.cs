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
        /// <summary>
        /// Protected constructor for deriving classes, which takes the .NET type
        /// that is to be converted from and to JSON.
        /// </summary>
        /// <param name="type">The .NET type that is to be converted from and to JSON.
        /// This may be null for converter implementations which are designed to be used only in
        /// a <see cref="JsonCustomConverterAttribute"/>.</param>
        protected ConverterBase(Type type)
        {
            Type = type;
        }

        /// <summary>
        /// The .NET type which is handled by this converter. This may be null
        /// for converter implementations which are designed to be used only in
        /// a <see cref="JsonCustomConverterAttribute"/>.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Is invoked by a parser or another producer whenever an
        /// array begins. This method must return an array consumer which
        /// is used to pass the array elements. Once, there are no more 
        /// elements, the <paramref name="whenDone"/> callback must be called 
        /// by the <see cref="IJsonArrayConsumer"/> implementation in order to pass the
        /// resulting .NET object.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON arrays, this method must be overridden.
        /// </summary>
        /// <param name="whenDone">A callback which must be used to pass
        /// the resulting .NET object, when Done() of the array consumer
        /// gets called.</param>
        /// <returns>An <see cref="IJsonArrayConsumer"/> implementation which
        /// is used to pass the elements of the array.</returns>
        public virtual IJsonArrayConsumer FromArray(Action<object> whenDone)
        {
            throw new Exception($"Cannot convert JSON array to type '{Type}'.");
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever a boolean
        /// value was parsed/produced.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON booleans, this method must be overridden.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <returns>The resulting .NET object.</returns>
        public virtual object FromBoolean(bool value)
        {
            throw new Exception($"Cannot convert JSON boolean to type '{Type}'.");
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever a 'null'
        /// value was parsed/produced.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON null values, this method must be overridden.
        /// </summary>
        /// <returns>The resulting .NET object.</returns>
        public virtual object FromNull()
        {
            throw new Exception($"Cannot convert JSON null to type '{Type}'.");
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever a floating
        /// point value was parsed/produced.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON floating point numbers, this method must be overridden.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        /// <returns>The resulting .NET object.</returns>
        public virtual object FromNumber(double value)
        {
            throw new Exception($"Cannot convert JSON floating point number to type '{Type}'.");
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever an integer
        /// value was parsed/produced.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON integer numbers, this method must be overridden.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The resulting .NET object.</returns>
        public virtual object FromNumber(long value)
        {
            throw new Exception($"Cannot convert JSON integer number to type '{Type}'.");
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever an unsigned integer
        /// value was parsed/produced.
        /// 
        /// This implementation in <see cref="ConverterBase"/> casts the unsigned
        /// integer to a signed integer and passes it to the other FromNumber overload.
        /// </summary>
        /// <param name="value">The unsigned integer value.</param>
        /// <returns>The resulting .NET object.</returns>
        public virtual object FromNumber(ulong value)
        {
            return FromNumber((long)value);
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever an object
        /// begins. This method must return an object consumer which
        /// is used to pass the property data of the object. Once, there are 
        /// no more properties, the <paramref name="whenDone"/> callback must 
        /// be called by the <see cref="IJsonObjectConsumer"/> implementation 
        /// in order to pass the resulting .NET object.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON objects, this method must be overridden.
        /// </summary>
        /// <param name="whenDone">A callback which must be used to pass
        /// the resulting .NET object, when Done() of the object consumer
        /// gets called.</param>
        /// <returns>An implementation of <see cref="IJsonObjectConsumer"/> which
        /// is used to consume the properties of the JSON object.</returns>
        public virtual IJsonObjectConsumer FromObject(Action<object> whenDone)
        {
            throw new Exception($"Cannot convert JSON object to type '{Type}'.");
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever a string
        /// value was parsed/produced.
        /// 
        /// This implementation in <see cref="ConverterBase"/> throws an exception. If a
        /// deriving converter accepts JSON strings, this method must be overridden.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The resulting .NET object.</returns>
        public virtual object FromString(string value)
        {
            throw new Exception($"Cannot convert JSON string to type '{Type}'.");
        }

        /// <summary>
        /// Writes data of the .NET object to the given <see cref="IJsonConsumer"/>
        /// in order to convert it to a JSON string or to any other representation of
        /// JSON data.
        /// </summary>
        /// <param name="value">The .NET object to write. Implementors may assume
        /// that the type of the incoming value can be cast to the target type 
        /// of this converter, however, it may be null.</param>
        /// <param name="writer">The JSON consumer which must be used to write 
        /// the data to according to the contents of the given value.</param>
        public abstract void Write(object value, IJsonConsumer writer);
    }
}
