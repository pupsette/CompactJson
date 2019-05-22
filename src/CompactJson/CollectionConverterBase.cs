using System;
using System.Collections;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    abstract class CollectionConverterBase : ConverterBase
    {
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
        /// Compares two converters for equality. It is used to avoid creation of
        /// converters for the same type.
        /// </summary>
        /// <returns>true, if the converters are equal; false, otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;

            return object.Equals(((CollectionConverterBase)obj).ElementConverter, ElementConverter);
        }

        /// <summary>
        /// Calculates a hash code for this converter. It is used to avoid creation of
        /// converters for the same type.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Type.GetHashCode() * 17 + ElementConverter.GetHashCode();
        }

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
