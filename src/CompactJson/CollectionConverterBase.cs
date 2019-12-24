using System;
using System.Collections;

namespace CompactJson
{
    /// <summary>
    /// A base class for converters which are capable of converting
    /// collections. It provides the ElementConverter property and
    /// an implementation of the <see cref="Write(object, IJsonConsumer)"/>
    /// method which writes a JSON array while expecting the given object
    /// to be <see cref="IEnumerable"/>.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    abstract class CollectionConverterBase : ConverterBase
    {
        /// <summary>
        /// The converter to be used for the elements of the collection. This cannot be null.
        /// </summary>
        public IConverter ElementConverter { get; }

        /// <summary>
        /// Protected constructor for deriving classes, which takes the .NET type
        /// that is to be converted from and to JSON.
        /// </summary>
        /// <param name="type">The .NET type that is to be converted from and to JSON.</param>
        /// <param name="elementConverter">The converter which is used for converting
        /// the individual items of the collection.</param>
        protected CollectionConverterBase(Type type, IConverter elementConverter)
            : base(type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            ElementConverter = elementConverter ?? throw new ArgumentNullException(nameof(elementConverter));
        }

        /// <summary>
        /// Writes data of the .NET object to the given <see cref="IJsonConsumer"/>
        /// in order to convert it to a JSON string or to any other representation of
        /// JSON data. This implementation expects <paramref name="value"/> to implement
        /// <see cref="IEnumerable"/> and creates a JSON array from it. The <paramref name="value"/>
        /// may be null, though.
        /// </summary>
        /// <param name="value">The .NET object to write. It may be null or an instance of
        /// <see cref="IEnumerable"/>.</param>
        /// <param name="writer">The JSON consumer which will be used to write the array to.</param>
        public override void Write(object value, IJsonConsumer writer)
        {
            if (value == null)
            {
                writer.Null();
                return;
            }

            var arrayConsumer = writer.Array();
            foreach (object item in (IEnumerable)value)
                ElementConverter.Write(item, arrayConsumer);
            arrayConsumer.Done();
        }
    }
}
