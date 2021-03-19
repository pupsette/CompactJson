using System;

namespace CompactJson
{
    /// <summary>
    /// Interface for converting .NET objects to JSON and back.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    interface IConverter
    {
        /// <summary>
        /// The .NET type which is handled by this converter. This
        /// is not needed, if this converter implementation was bound to a
        /// property or class using the <see cref="JsonCustomConverterAttribute"/>.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Is invoked by a parser or another producer whenever an
        /// array begins. This method must return an array consumer which
        /// is used to pass the array elements. Once, there are no more 
        /// elements, the <paramref name="whenDone"/> callback must be called 
        /// by the <see cref="IJsonArrayConsumer"/> implementation in order to pass the
        /// resulting .NET object.
        /// </summary>
        /// <param name="whenDone">A callback which must be used to pass
        /// the resulting .NET object, when Done() of the array consumer
        /// gets called.</param>
        /// <returns>An <see cref="IJsonArrayConsumer"/> implementation which
        /// is used to pass the elements of the array.</returns>
        IJsonArrayConsumer FromArray(Action<object> whenDone);

        /// <summary>
        /// Is invoked by a parser or another producer whenever a boolean
        /// value was parsed/produced.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <returns>The resulting .NET object.</returns>
        object FromBoolean(bool value);

        /// <summary>
        /// Is invoked by a parser or another producer whenever a 'null'
        /// value was parsed/produced.
        /// </summary>
        /// <returns>The resulting .NET object.</returns>
        object FromNull();

        /// <summary>
        /// Is invoked by a parser or another producer whenever a floating
        /// point value was parsed/produced.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        /// <returns>The resulting .NET object.</returns>
        object FromNumber(double value);

        /// <summary>
        /// Is invoked by a parser or another producer whenever an integer
        /// value was parsed/produced.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The resulting .NET object.</returns>
        object FromNumber(long value);

        /// <summary>
        /// Is invoked by a parser or another producer whenever an unsigned integer
        /// value was parsed/produced.
        /// </summary>
        /// <param name="value">The unsigned integer value.</param>
        /// <returns>The resulting .NET object.</returns>
        object FromNumber(ulong value);

        /// <summary>
        /// Is invoked by a parser or another producer whenever an object
        /// begins. This method must return an object consumer which
        /// is used to pass the property data of the object. Once, there are 
        /// no more properties, the <paramref name="whenDone"/> callback must 
        /// be called by the <see cref="IJsonObjectConsumer"/> implementation 
        /// in order to pass the resulting .NET object.
        /// </summary>
        /// <param name="whenDone">A callback which must be used to pass
        /// the resulting .NET object, when Done() of the object consumer
        /// gets called.</param>
        /// <returns>An <see cref="IJsonObjectConsumer"/> implementation which
        /// is used to pass the elements of the array.</returns>
        IJsonObjectConsumer FromObject(Action<object> whenDone);

        /// <summary>
        /// Is invoked by a parser or another producer whenever a string
        /// value was parsed/produced.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The resulting .NET object.</returns>
        object FromString(string value);

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
        void Write(object value, IJsonConsumer writer);
    }
}
