using System;
using System.IO;
using System.Text;

namespace CompactJson
{
#if COMPACTJSON_PUBLIC
    public
#endif
    class JsonString : JsonValue
    {
        public JsonString(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            Value = value;
        }

        public string Value { get; }

        public override void Write(IJsonConsumer consumer)
        {
            consumer.String(Value);
        }

        public static string Escape(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length + 5);
            Escape(s, sb);
            return sb.ToString();
        }

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
