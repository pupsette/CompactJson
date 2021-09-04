using System;
using System.Text;

namespace CompactJson
{
    /// <summary>
    /// A converter for converting DateTime objects to string and back.
    /// The serialized DateTime string is ISO 8601 conformant, however
    /// it does not convert all possible ISO 8601 representations to
    /// a DateTime object.
    /// 
    /// Allowed formats (examples):
    ///  * 2010-08-22T09:15:00
    ///  * 2010-08-22T09:15:00.910
    ///  * 2010-08-22T09:15:00Z
    ///  * 2010-08-22T09:15:00.910Z
    ///  * 2010-08-22T09:15:00+01:30
    ///  * 2010-08-22T09:15:00-03:00
    ///  * 2010-08-22T09:15:00.911+01:30
    ///  * 2010-08-22T09:15:00.911-03:00
    ///  
    /// When serializing, the milliseconds part will be omitted, if this part is 000.
    /// Also the 'Z' indicator will only be appended, if the DateTime object has 
    /// the DateTimeKind.Utc. The UTC offset (+/-) will only be appended, if the DateTime object has 
    /// the DateTimeKind.Local. If the DateTime object has DateTimeKind.Unspecified, there
    /// will be no suffix appended.
    /// 
    /// When deserializing, one of the above formats is expected, otherwise an exception
    /// is thrown. The resulting DateTime object will have the DateTimeKind set according
    /// to the encountered suffix. Keep in mind, that serializer and deserializer 
    /// might have different UTC offsets. In this case, you cannot expect the deserialization
    /// (FromString method) and subsequent serialization (Write method) to reproduce the 
    /// input string.
    /// 
    /// </summary>
    internal sealed class DateTimeConverter : NullableConverterBase<DateTime>
    {
        /// <summary>
        /// Instantiates a <see cref="DateTimeConverter"/>.
        /// </summary>
        /// <param name="nullable">A flag indicating, whether this converter
        /// handles a nullable <see cref="DateTime"/> or not.</param>
        public DateTimeConverter(bool nullable) : base(nullable)
        {
        }

        protected override void InternalWrite(DateTime value, IJsonConsumer writer)
        {
            StringBuilder sb = new StringBuilder(50);
            if (value.Year < 10)
                sb.Append("000");
            else if (value.Year < 100)
                sb.Append("00");
            else if (value.Year < 1000)
                sb.Append('0');
            sb.Append(value.Year);

            sb.Append('-');
            Append2Chars(sb, value.Month);
            sb.Append('-');
            Append2Chars(sb, value.Day);
            sb.Append('T');
            Append2Chars(sb, value.Hour);
            sb.Append(':');
            Append2Chars(sb, value.Minute);
            sb.Append(':');
            Append2Chars(sb, value.Second);

            if (value.Millisecond > 0)
            {
                sb.Append('.');
                Append3Chars(sb, value.Millisecond);
            }

            if (value.Kind == DateTimeKind.Utc)
                sb.Append('Z');
            else if (value.Kind == DateTimeKind.Local)
            {
                TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(value);
                if (offset >= TimeSpan.Zero)
                    sb.Append('+');
                else
                {
                    sb.Append('-');
                    offset = offset.Negate();
                }
                Append2Chars(sb, offset.Hours);
                sb.Append(':');
                Append2Chars(sb, offset.Minutes);
            }
            writer.String(sb.ToString());
        }

        private static void Append2Chars(StringBuilder sb, int value)
        {
            if (value < 10)
                sb.Append('0');
            sb.Append(value);
        }

        private static void Append3Chars(StringBuilder sb, int value)
        {
            if (value < 10)
                sb.Append("00");
            else if (value < 100)
                sb.Append('0');
            sb.Append(value);
        }

        private static void Append4Chars(StringBuilder sb, int value)
        {
            if (value < 10)
                sb.Append("000");
            else if (value < 100)
                sb.Append("00");
            else if (value < 1000)
                sb.Append('0');
            sb.Append(value);
        }

        private static int ParseChars(string str, int startIndex, int count, int dim)
        {
            if (startIndex + count > str.Length)
                ThrowOnInvalidDateTimeFormat(str);

            int result = 0;
            for (int i = 0; i < count; i++)
            {
                int c = (int)str[startIndex + i] - (int)'0';
                if (c < 0 || c > 9)
                    ThrowOnInvalidDateTimeFormat(str);

                result += c * dim;
                dim /= 10;
            }
            return result;
        }

        private static bool HasChar(string str, int index, char expected)
        {
            return (index < str.Length && str[index] == expected);
        }

        private static void ExpectChar(string str, int index, char expected)
        {
            if (!HasChar(str, index, expected))
                ThrowOnInvalidDateTimeFormat(str);
        }

        private static void ThrowOnInvalidDateTimeFormat(string str)
        {
            throw new FormatException($"String '{str}' was not recognized as a valid DateTime.");
        }

        public override object FromString(string value)
        {
            int year = ParseChars(value, 0, 4, 1000);
            ExpectChar(value, 4, '-');
            int month = ParseChars(value, 5, 2, 10);
            ExpectChar(value, 7, '-');
            int day = ParseChars(value, 8, 2, 10);
            ExpectChar(value, 10, 'T');
            int hour = ParseChars(value, 11, 2, 10);
            ExpectChar(value, 13, ':');
            int minute = ParseChars(value, 14, 2, 10);
            ExpectChar(value, 16, ':');
            int second = ParseChars(value, 17, 2, 10);

            int index = 19;
            int milliseconds = 0;
            if (HasChar(value, index, '.'))
            {
                index++;
                double fraction = 0;
                double dim = 10.0;
                while (index < value.Length && char.IsDigit(value[index]))
                {
                    int c = (int)value[index] - (int)'0';
                    fraction += c / dim;
                    dim *= 10;
                    index++;
                }
                milliseconds = Math.Min(999, (int)(1000 * fraction + 0.5));
            }

            if (index < value.Length)
            {
                DateTime dtUtc = new DateTime(year, month, day, hour, minute, second, milliseconds, DateTimeKind.Utc);
                if (value[index] == 'Z')
                {
                    if (index + 1 > value.Length)
                        ThrowOnInvalidDateTimeFormat(value);
                    return dtUtc;
                }
                else if (value[index] == '+' || value[index] == '-')
                {
                    if (index + 6 > value.Length)
                        ThrowOnInvalidDateTimeFormat(value);

                    int offsetHours = ParseChars(value, index + 1, 2, 10);
                    ExpectChar(value, index + 3, ':');
                    int offsetMinutes = ParseChars(value, index + 4, 2, 10);
                    TimeSpan offset = new TimeSpan(offsetHours, offsetMinutes, 0);

                    if (value[index] == '+')
                        return dtUtc.Add(-offset).ToLocalTime();
                    return dtUtc.Add(offset).ToLocalTime();
                }
                else
                    ThrowOnInvalidDateTimeFormat(value);
            }

            return new DateTime(year, month, day, hour, minute, second, milliseconds, DateTimeKind.Unspecified);
        }
    }
}
