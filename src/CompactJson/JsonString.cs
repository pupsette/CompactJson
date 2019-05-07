using System;
using System.Text;

namespace CompactJson
{
    /// <summary>
    /// A class representing a JSON string within the
    /// generic JSON object model.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class JsonString : JsonValue
    {
        /// <summary>
        /// Constructs a JSON string value.
        /// </summary>
        /// <param name="value">The string. This must not be null.</param>
        public JsonString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            Value = value;
        }

        /// <summary>
        /// The string contents of this JSON string.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Writes the string contents to the given <see cref="IJsonConsumer"/>.
        /// </summary>
        /// <param name="consumer">The consumer.</param>
        public override void Write(IJsonConsumer consumer)
        {
            consumer.String(Value);
        }

        /// <summary>
        /// Escapes the given string according to the JSON specification. This will
        /// also add double quotes at start and end. The given string must not be null.
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <returns>The escaped string including double quotes.</returns>
        public static string Escape(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length + 5);
            Escape(s, sb);
            return sb.ToString();
        }

        /// <summary>
        /// Escapes the given string according to the JSON specification. This will
        /// also add double quotes at start and end. The given string must not be null.
        /// </summary>
        /// <param name="s">The string to escape.</param>
        /// <param name="sb">An existing string builder to write the escaped string to.</param>
        public static void Escape(string s, StringBuilder sb)
        {
            sb.Append('"');
            int runIndex = -1;
            int l = s.Length;
            for (var index = 0; index < l; ++index)
            {
                var c = s[index];

                if (c != '\t' && c != '\n' && c != '\r' && c != '\"' && c != '\\' && c != '\0')
                {
                    if (runIndex == -1)
                        runIndex = index;

                    continue;
                }

                if (runIndex != -1)
                {
                    sb.Append(s, runIndex, index - runIndex);
                    runIndex = -1;
                }

                switch (c)
                {
                    case '\t': sb.Append("\\t"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '"':
                    case '\\': sb.Append('\\'); sb.Append(c); break;
                    case '\0': sb.Append("\\u0000"); break;
                    default:
                        sb.Append(c);
                        break;
                }
            }

            if (runIndex != -1)
                sb.Append(s, runIndex, s.Length - runIndex);

            sb.Append('"');
        }
    }
}
