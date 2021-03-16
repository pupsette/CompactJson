using System;
using System.IO;

namespace CompactJson
{
    /// <summary>
    /// This class represents a JSON number.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonNumber : JsonValue
    {
        private readonly object value;

        /// <summary>
        /// Initializes a JSON number from a double.
        /// </summary>
        /// <param name="value">The double value.</param>
        public JsonNumber(double value)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a JSON number from a signed integer.
        /// </summary>
        /// <param name="value">The signed integer.</param>
        public JsonNumber(long value)
        {
            this.value = value;
        }

        /// <summary>
        /// Initializes a JSON number from an unsigned integer.
        /// </summary>
        /// <param name="value">The unsigned integer.</param>
        public JsonNumber(ulong value)
        {
            this.value = value;
        }

        /// <summary>
        /// Returns true, if the underlying value is a double value. This depends on
        /// how this JSON number is constructed. It may even return true, if the
        /// represented floating point value is integral.
        /// </summary>
        public bool IsDouble { get => value is double; }

        /// <summary>
        /// Returns true, if the underlying value is an integer. This depends on
        /// how this JSON number is constructed. E.g. it will return false, if the
        /// underyling value is a double even though its value is integral.
        /// </summary>
        public bool IsInteger { get => !IsDouble; }

        /// <summary>
        /// Returns true, if the underlying value is a signed integer. This depends on
        /// how this JSON number is constructed. E.g. it will return false, if the
        /// underyling value is a double even though its value is integral.
        /// </summary>
        public bool IsSignedInteger { get => value is long; }

        /// <summary>
        /// Returns true, if the underlying value is a unsigned integer. This depends on
        /// how this JSON number is constructed. E.g. it will return false, if the
        /// underyling value is a double even though its value is integral.
        /// </summary>
        public bool IsUnsignedInteger { get => value is ulong; }

        /// <summary>
        /// The underlying value. This can be either be a double, a long or an ulong.
        /// </summary>
        public object Value { get => value; }

        /// <summary>
        /// Returns the JSON number as floating point value.
        /// </summary>
        /// <returns>The floating point value.</returns>
        public double AsDouble()
        {
            if (value is double d)
                return d;
            if (value is long l)
                return l;
            return (ulong)value;
        }

        /// <summary>
        /// Returns the JSON number as 64 bit signed integer value.
        /// </summary>
        /// <returns>The integer value.</returns>
        public long AsInteger(bool throwOnOverflow = false)
        {
            if (value is long l)
                return l;
            if (value is double d)
            {
                if (throwOnOverflow && (d < long.MinValue || d > long.MaxValue))
                    throw new Exception($"Floating point number {d} cannot be converted to a signed 64 bit integer, because its value is out of range.");
                return (long)d;
            }
            ulong ul = (ulong)value;
            if (throwOnOverflow && ul > long.MaxValue)
                throw new Exception($"Unsigned integer number {ul} cannot be converted to a signed 64 bit integer, because its value is out of range.");
            return (long)ul;
        }

        /// <summary>
        /// Returns the JSON number as 64 bit unsigned integer value.
        /// </summary>
        /// <returns>The unsigned integer value.</returns>
        public ulong AsUnsignedInteger(bool throwIfNegative = true)
        {
            if (value is ulong l)
                return l;
            if (value is double d)
            {
                if (throwIfNegative && d < 0.0)
                    throw new Exception($"Negative floating point number {d} cannot be converted to an unsigned integer.");
                return (ulong)d;
            }
            long sl = (long)value;
            if (throwIfNegative && sl < 0)
                throw new Exception($"Negative integer number {sl} cannot be converted to an unsigned integer.");
            return (ulong)sl;
        }

        /// <summary>
        /// Writes this <see cref="JsonNumber"/> to a <see cref="IJsonConsumer"/>.
        /// This is also used internally by <see cref="JsonValue.Write(TextWriter, bool)"/>, 
        /// <see cref="JsonValue.ToModel(Type)"/> and <see cref="JsonValue.ToModel{T}"/> in order to convert
        /// this generic object model to a JSON string or another .NET object.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            if (value is ulong ul)
                consumer.Number(ul);
            else if (value is double d)
                consumer.Number(d);
            else
                consumer.Number((long)value);
        }
    }
}
