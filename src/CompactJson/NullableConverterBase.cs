using System;

namespace CompactJson
{
    /// <summary>
    /// A base class for nullable types. This is useful to instantiate
    /// the same converter implementation for a non-nullable type
    /// and its nullable variant.
    /// </summary>
    /// <typeparam name="T">The non-nullable type.</typeparam>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    abstract class NullableConverterBase<T> : ConverterBase where T : struct
    {
        private readonly bool mNullable;

        /// <summary>
        /// Instantiates a <see cref="NullableConverterBase{T}"/>.
        /// </summary>
        /// <param name="nullable">A flag indicating, whether to convert
        /// <see cref="Nullable{T}"/> or just <typeparamref name="T"/>.</param>
        protected NullableConverterBase(bool nullable)
            : base(nullable ? typeof(T?) : typeof(T))
        {
            mNullable = nullable;
        }

        /// <summary>
        /// Is invoked by a parser or another producer whenever a 'null'
        /// value was parsed/produced.
        /// </summary>
        /// <returns>The resulting .NET object.</returns>
        public sealed override object FromNull()
        {
            if (mNullable)
                return null;

            return base.FromNull();
        }

        /// <summary>
        /// Writes data of the .NET object to the given <see cref="IJsonConsumer"/>
        /// in order to convert it to a JSON string or to any other representation of
        /// JSON data.
        /// </summary>
        /// <param name="value">The .NET object to write. The object type must match
        /// the type of the converter.</param>
        /// <param name="writer">The JSON consumer which will be used to write 
        /// the data to according to the contents of the given value.</param>
        public sealed override void Write(object value, IJsonConsumer writer)
        {
            if (mNullable && value == null)
            {
                writer.Null();
                return;
            }

            InternalWrite((T)value, writer);
        }

        /// <summary>
        /// An internal Write method which needs to be implemented 
        /// by deriving classes. The <paramref name="value"/> is non-nullable 
        /// at this point.
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <param name="writer">The json consumer to write to.</param>
        protected abstract void InternalWrite(T value, IJsonConsumer writer);
    }
}
