using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace CompactJson
{
    /// <summary>
    /// A static class which exposes serialization/deserialization methods.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    static class Serializer
    {
        /// <summary>
        /// Parses the given JSON and converts it to a generic JsonValue.
        /// </summary>
        /// <param name="json">The JSON to parse.</param>
        /// <returns>The generic JsonValue.</returns>
        public static JsonValue Parse(string json)
        {
            using (StringReader reader = new StringReader(json))
                return Parse(reader);
        }

        /// <summary>
        /// Parses JSON read from the given text reader and converts 
        /// it to a generic JsonValue.
        /// </summary>
        /// <param name="reader">The TextReader to read JSON from.</param>
        /// <returns>The generic JsonValue.</returns>
        public static JsonValue Parse(TextReader reader)
        {
            JsonValueJsonConsumer valueParser = new JsonValueJsonConsumer();
            Parse(reader, valueParser);
            return valueParser.Result;
        }

        /// <summary>
        /// Parses the given JSON and pushes all
        /// data into the given consumer.
        /// </summary>
        /// <param name="json">The JSON to parse.</param>
        /// <param name="consumer">The consumer which will be invoked for the
        /// parsed fragments.</param>
        public static void Parse(string json, IJsonConsumer consumer)
        {
            using (StringReader reader = new StringReader(json))
                Parse(reader, consumer);
        }

        /// <summary>
        /// Parses JSON read from the given text reader and pushes all
        /// data into the given consumer.
        /// </summary>
        /// <param name="reader">The TextReader to read JSON from.</param>
        /// <param name="consumer">The consumer which will be invoked for the
        /// parsed fragments.</param>
        public static void Parse(TextReader reader, IJsonConsumer consumer)
        {
            var tokenizer = new Tokenizer(reader);
            tokenizer.MoveNext();
            Parser.ParseValue(tokenizer, consumer);

            if (tokenizer.CurrentToken.Type != Tokenizer.Token.None)
                throw new ParserException($"Unexpected input data '{tokenizer.CurrentToken}' after end of JSON entity in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);
        }

        /// <summary>
        /// Parses JSON read from the given text reader and converts it into
        /// an object of the given type. The resulting output can be cast
        /// safely to that given type.
        /// </summary>
        /// <param name="reader">The TextReader to read JSON from.</param>
        /// <param name="modelType">The type of the result object.</param>
        /// <returns>The result object. For class types, this can be null.</returns>
        public static object Parse(TextReader reader, Type modelType)
        {
            var consumer = new ModelJsonConsumer(ConverterRegistry.Get(modelType));
            Parse(reader, consumer);
            return consumer.Result;
        }

        /// <summary>
        /// Parses the given JSON and converts it into
        /// an object of the given type. The resulting output can be cast
        /// safely to that given type.
        /// </summary>
        /// <param name="json">The JSON to parse.</param>
        /// <param name="modelType">The type of the result object.</param>
        /// <returns>The result object. For class types, this can be null.</returns>
        public static object Parse(string json, Type modelType)
        {
            using (StringReader reader = new StringReader(json))
                return Parse(reader, modelType);
        }

        /// <summary>
        /// Parses the given JSON and converts it into
        /// an object of the given type. The resulting output can be cast
        /// safely to that given type.
        /// </summary>
        /// <typeparam name="T">The type of the result object.</typeparam>
        /// <param name="json">The JSON to parse.</param>
        /// <returns>The result object. For class types, this can be null.</returns>
        public static T Parse<T>(string json)
        {
            using (var reader = new StringReader(json))
                return (T)Parse(reader, typeof(T));
        }

        /// <summary>
        /// Parses JSON read from the given text reader and converts it into
        /// an object of the given type. The resulting output can be cast
        /// safely to that given type.
        /// </summary>
        /// <typeparam name="T">The type of the result object.</typeparam>
        /// <param name="reader">The TextReader to read JSON from.</param>
        /// <returns>The result object. For class types, this can be null.</returns>
        public static T Parse<T>(TextReader reader)
        {
            return (T)Parse(reader, typeof(T));
        }

        /// <summary>
        /// Serializes the given object to JSON and writes the result into
        /// the given TextWriter.
        /// </summary>
        /// <param name="model">The object to serialize. This may be null or a primitive.</param>
        /// <param name="writer">The TextWriter to write the serialized JSON to.</param>
        /// <param name="indentation">A flag indicating whether the resulting JSON should be pretty-printed or not.</param>
        public static void Write(object model, TextWriter writer, bool indentation)
        {
            var consumer = new TextWriterJsonConsumer(writer, indentation);
            Write(model, consumer);
        }

        /// <summary>
        /// Serializes the given object into the given consumer.
        /// </summary>
        /// <param name="model">The object to serialize. This may be null or a primitive.</param>
        /// <param name="consumer">An instance of IJsonConsumer which receives all data from
        /// the given object.</param>
        public static void Write(object model, IJsonConsumer consumer)
        {
            if (model == null)
                consumer.Null();
            else
                ConverterRegistry.Get(model.GetType()).Write(model, consumer);
        }

        /// <summary>
        /// Serializes the given object to a JSON string.
        /// </summary>
        /// <param name="model">The object to serialize. This may be null or a primitive.</param>
        /// <param name="indentation">A flag indicating whether the resulting JSON should be pretty-printed or not.</param>
        /// <returns>The serialized JSON.</returns>
        public static string ToString(object model, bool indentation)
        {
            using (var result = new StringWriter())
            {
                Write(model, result, indentation);
                return result.ToString();
            }
        }

        private static class Parser
        {
            public static void ParseObject(Tokenizer tokenizer, IJsonObjectConsumer consumer)
            {
                tokenizer.MoveNext(); // skip '{'
                if (tokenizer.CurrentToken.Type == Tokenizer.Token.CurlyClose)
                {
                    tokenizer.MoveNext(); // skip '}'
                    consumer.Done();
                    return;
                }

                for (; ; )
                {
                    if (tokenizer.CurrentToken.Type == Tokenizer.Token.None)
                        throw new ParserException($"Unexpected end of stream reached while parsing object in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                    if (tokenizer.CurrentToken.Type != Tokenizer.Token.String)
                        throw new ParserException($"Unexpected token in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}. Property name was expected.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                    // expect string literal
                    consumer.PropertyName(tokenizer.CurrentToken.StringValue);

                    // expect ':'
                    tokenizer.MoveNext();
                    if (tokenizer.CurrentToken.Type == Tokenizer.Token.None)
                        throw new ParserException($"Unexpected end of stream reached while parsing object in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);
                    if (tokenizer.CurrentToken.Type != Tokenizer.Token.Colon)
                        throw new ParserException($"Unexpected token in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}. ':' was expected.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                    // parse value
                    tokenizer.MoveNext(); // skip ':'
                    ParseValue(tokenizer, consumer);

                    if (tokenizer.CurrentToken.Type == Tokenizer.Token.None)
                        throw new ParserException($"Unexpected end of stream reached while parsing object in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);
                    if (tokenizer.CurrentToken.Type == Tokenizer.Token.CurlyClose)
                    {
                        tokenizer.MoveNext(); // skip '}'
                        consumer.Done();
                        return;
                    }
                    if (tokenizer.CurrentToken.Type != Tokenizer.Token.Comma)
                        throw new ParserException($"Unexpected token in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}. Either '}}' or ',' was expected.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                    tokenizer.MoveNext(); // skip ','
                }
            }

            public static void ParseArray(Tokenizer tokenizer, IJsonArrayConsumer consumer)
            {
                tokenizer.MoveNext(); // skip '['

                if (tokenizer.CurrentToken.Type == Tokenizer.Token.None)
                    throw new ParserException($"Unexpected end of stream reached while parsing array in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                if (tokenizer.CurrentToken.Type == Tokenizer.Token.SquaredClose)
                {
                    tokenizer.MoveNext(); // skip ']'
                    consumer.Done();
                    return;
                }

                for (; ; )
                {
                    // parse value
                    ParseValue(tokenizer, consumer);

                    if (tokenizer.CurrentToken.Type == Tokenizer.Token.Comma)
                        tokenizer.MoveNext();
                    else if (tokenizer.CurrentToken.Type == Tokenizer.Token.SquaredClose)
                    {
                        tokenizer.MoveNext(); // skip ']'
                        consumer.Done();
                        return;
                    }
                    else if (tokenizer.CurrentToken.Type == Tokenizer.Token.None)
                        throw new ParserException($"Unexpected end of stream reached while parsing array in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);
                    else
                        throw new ParserException($"Unexpected token in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}. Either ']' or ',' was expected.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);
                }
            }

            public static void ParseValue(Tokenizer tokenizer, IJsonConsumer consumer)
            {
                if (tokenizer.CurrentToken.Type == Tokenizer.Token.None)
                    throw new ParserException($"Unexpected end of stream reached in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                if (tokenizer.CurrentToken.Type == Tokenizer.Token.CurlyOpen)
                {
                    ParseObject(tokenizer, consumer.Object());
                    return;
                }
                if (tokenizer.CurrentToken.Type == Tokenizer.Token.SquaredOpen)
                {
                    ParseArray(tokenizer, consumer.Array());
                    return;
                }

                if (tokenizer.CurrentToken.Type == Tokenizer.Token.String)
                    consumer.String(tokenizer.CurrentToken.StringValue);
                else if (tokenizer.CurrentToken.Type == Tokenizer.Token.Boolean)
                    consumer.Boolean(tokenizer.CurrentToken.BooleanValue);
                else if (tokenizer.CurrentToken.Type == Tokenizer.Token.NumberInteger)
                    consumer.Number(tokenizer.CurrentToken.IntegerValue);
                else if (tokenizer.CurrentToken.Type == Tokenizer.Token.NumberUnsignedInteger)
                    consumer.Number(tokenizer.CurrentToken.UnsignedIntegerValue);
                else if (tokenizer.CurrentToken.Type == Tokenizer.Token.NumberFloat)
                    consumer.Number(tokenizer.CurrentToken.FloatValue);
                else if (tokenizer.CurrentToken.Type == Tokenizer.Token.Null)
                    consumer.Null();
                else
                    throw new ParserException($"Expected value in line {tokenizer.CurrentToken.LineNo} at position {tokenizer.CurrentToken.Position}, but found '{tokenizer.CurrentToken}'.", tokenizer.CurrentToken.LineNo, tokenizer.CurrentToken.Position);

                tokenizer.MoveNext(); // skip value literal
            }
        }

        #region TOKENIZER

        private class Tokenizer
        {
            private readonly TextReader mReader;
            private const int BUFFER_SIZE = 1024;
            private readonly char[] mBuffer = new char[BUFFER_SIZE];
            private int mBufferEnd = BUFFER_SIZE + 1;
            private int mCurrentIndex;
            private readonly StringBuilder mStringBuilder = new StringBuilder();
            private int lineNo = 1;
            private int columnIndex = 0;

            public TokenData CurrentToken;
            public TokenData NextToken;

            public enum Token
            {
                None = 0,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                NumberFloat,
                NumberInteger,
                NumberUnsignedInteger,
                Boolean,
                Null
            }

            public struct TokenData
            {
                public Token Type;
                public string StringValue;
                public bool BooleanValue;
                public double FloatValue;
                public long IntegerValue;
                public ulong UnsignedIntegerValue;
                public int LineNo;
                public int Position;

                public override string ToString()
                {
                    switch (Type)
                    {
                        case Token.NumberFloat:
                            return FloatValue.ToString(CultureInfo.InvariantCulture);
                        case Token.NumberInteger:
                            return IntegerValue.ToString(CultureInfo.InvariantCulture);
                        case Token.NumberUnsignedInteger:
                            return UnsignedIntegerValue.ToString(CultureInfo.InvariantCulture);
                        case Token.String:
                            return "\"" + JsonString.Escape(StringValue) + "\"";
                        case Token.Boolean:
                            return BooleanValue ? "true" : "false";
                        case Token.Colon:
                            return ":";
                        case Token.Comma:
                            return ",";
                        case Token.Null:
                            return "null";
                        case Token.CurlyOpen:
                            return "{";
                        case Token.CurlyClose:
                            return "}";
                        case Token.SquaredOpen:
                            return "[";
                        case Token.SquaredClose:
                            return "]";
                        case Token.None:
                            return "end of stream";
                        default:
                            return Type.ToString();
                    }
                }
            }

            public Tokenizer(TextReader reader)
            {
                mReader = reader;

                int read = mReader.ReadBlock(mBuffer, 0, BUFFER_SIZE);
                if (read < BUFFER_SIZE)
                    mBufferEnd = read;

                MoveNext();
            }

            private void ReadNextChunk()
            {
                mCurrentIndex = 0;
                int read = mReader.ReadBlock(mBuffer, 0, BUFFER_SIZE);
                if (read < BUFFER_SIZE)
                    mBufferEnd = read;
            }

            private char PeekChar()
            {
                if (mCurrentIndex >= mBufferEnd)
                    return (char)0;

                return mBuffer[mCurrentIndex];
            }

            private char NextChar()
            {
                if (mCurrentIndex >= mBufferEnd)
                    return (char)0;

                char c = mBuffer[mCurrentIndex++];
                columnIndex++;
                if (mCurrentIndex == BUFFER_SIZE)
                    ReadNextChunk();

                if (c == '\n')
                {
                    lineNo++;
                    columnIndex = 0;
                }

                return c;
            }

            private bool IsWhitespace(char c)
            {
                return (c == ' ' || c == '\t' || c == '\r' || c == '\n');
            }

            public void MoveNext()
            {
                CurrentToken = NextToken;
                for (; ; )
                {
                    char c = NextChar();
                    if (c == (char)0)
                    {
                        NextToken.LineNo = lineNo;
                        NextToken.Position = columnIndex;
                        NextToken.Type = Token.None;
                        return;
                    }
                    if (IsWhitespace(c))
                        continue;

                    if (c == '/')
                    {
                        Expect("//");
                        while ((c = NextChar()) != (char)0)
                        {
                            if (c == '\n') // comment till end of line
                                break;
                        }
                    }
                    else
                    {
                        NextToken.LineNo = lineNo;
                        NextToken.Position = columnIndex;

                        if (c == 't')
                        {
                            Expect("true");
                            NextToken.Type = Token.Boolean;
                            NextToken.BooleanValue = true;
                            return;
                        }
                        else if (c == 'n')
                        {
                            Expect("null");
                            NextToken.Type = Token.Null;
                            return;
                        }
                        else if (c == 'f')
                        {
                            Expect("false");
                            NextToken.Type = Token.Boolean;
                            NextToken.BooleanValue = false;
                            return;
                        }
                        else if (c == '-' || c == '+' || char.IsDigit(c))
                        {
                            ParseNumber(c, ref NextToken);
                            return;
                        }
                        else if (c == '\"')
                        {
                            NextToken.Type = Token.String;
                            NextToken.StringValue = ParseString();
                            return;
                        }
                        else if (c == '{')
                        {
                            NextToken.Type = Token.CurlyOpen;
                            return;
                        }
                        else if (c == '}')
                        {
                            NextToken.Type = Token.CurlyClose;
                            return;
                        }
                        else if (c == '[')
                        {
                            NextToken.Type = Token.SquaredOpen;
                            return;
                        }
                        else if (c == ']')
                        {
                            NextToken.Type = Token.SquaredClose;
                            return;
                        }
                        else if (c == ':')
                        {
                            NextToken.Type = Token.Colon;
                            return;
                        }
                        else if (c == ',')
                        {
                            NextToken.Type = Token.Comma;
                            return;
                        }
                        else
                        {
                            if (c < 32)
                                throw new ParserException($"Unexpected control character 0x{(int)c:X2} in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                            else
                                throw new ParserException($"Unexpected character '{c}' in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                        }
                    }
                }
            }

            private string ParseString()
            {
                mStringBuilder.Clear();
                bool escaped = false;
                for (; ; )
                {
                    char c = NextChar();
                    if (c == (char)0)
                        throw new ParserException($"Unexpected end of stream reached while parsing string in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);

                    if (escaped)
                    {
                        switch (c)
                        {
                            case '"':
                                mStringBuilder.Append('"');
                                break;
                            case '\\':
                                mStringBuilder.Append('\\');
                                break;
                            case '/':
                                mStringBuilder.Append('/');
                                break;
                            case 'b':
                                mStringBuilder.Append('\b');
                                break;
                            case 'f':
                                mStringBuilder.Append('\f');
                                break;
                            case 'n':
                                mStringBuilder.Append('\n');
                                break;
                            case 'r':
                                mStringBuilder.Append('\r');
                                break;
                            case 't':
                                mStringBuilder.Append('\t');
                                break;
                            case 'u':
                                {
                                    char hex1 = NextChar();
                                    char hex2 = NextChar();
                                    char hex3 = NextChar();
                                    char hex4 = NextChar();
                                    if (hex1 == 0 || hex2 == 0 || hex3 == 0 || hex4 == 0)
                                        throw new ParserException($"Unexpected end of stream reached while parsing unicode escape sequence in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);

                                    // parse the 32 bit hex into an integer codepoint
                                    uint codePoint = ParseUnicode(hex1, hex2, hex3, hex4);
                                    mStringBuilder.Append((char)codePoint);
                                }
                                break;
                            default:
                                throw new ParserException($"Invalid character '{c}' in escape sequence in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                        }
                        escaped = false;
                    }
                    else if (c == '"')
                    {
                        return mStringBuilder.ToString();
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else
                    {
                        if (c < 32)
                            throw new ParserException($"Unexpected control character 0x{(int)c:X2} in string literal in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);

                        mStringBuilder.Append(c);
                    }
                }
            }

            private uint ParseSingleChar(char c1, uint multipliyer)
            {
                uint p1 = 0;
                if (c1 >= '0' && c1 <= '9')
                    p1 = (uint)(c1 - '0') * multipliyer;
                else if (c1 >= 'A' && c1 <= 'F')
                    p1 = (uint)((c1 - 'A') + 10) * multipliyer;
                else if (c1 >= 'a' && c1 <= 'f')
                    p1 = (uint)((c1 - 'a') + 10) * multipliyer;
                return p1;
            }

            private uint ParseUnicode(char c1, char c2, char c3, char c4)
            {
                uint p1 = ParseSingleChar(c1, 0x1000);
                uint p2 = ParseSingleChar(c2, 0x100);
                uint p3 = ParseSingleChar(c3, 0x10);
                uint p4 = ParseSingleChar(c4, 1);

                return p1 + p2 + p3 + p4;
            }

            private void Expect(string expected)
            {
                for (int i = 1; i < expected.Length; i++)
                {
                    char c = NextChar();
                    if (c == 0 && (mCurrentIndex >= mBufferEnd))
                        throw new ParserException($"Unexpected end of stream reached while '{expected}' was expected in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);

                    if (c != expected[i])
                        throw new ParserException($"Expected '{expected}' at position {columnIndex} in line {lineNo}, but found '{expected.Substring(0, i) + c}'.", lineNo, columnIndex);
                }
            }

            private void ParseNumber(char c, ref TokenData tokenData)
            {
                mStringBuilder.Clear();
                mStringBuilder.Append(c);
                bool dotAppeared = false;
                bool exponentAppeared = false;
                bool minusAppeared = c == '-';
                for (; ; )
                {
                    c = PeekChar();
                    if (char.IsDigit(c))
                    {
                        mStringBuilder.Append(NextChar());
                    }
                    else if (c == '.')
                    {
                        mStringBuilder.Append(NextChar());
                        if (dotAppeared)
                            throw new ParserException($"The dot '.' must not appear twice in the number literal {mStringBuilder} in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                        dotAppeared = true;
                    }
                    else if (c == 'E' || c == 'e')
                    {
                        exponentAppeared = true;
                        mStringBuilder.Append(NextChar());
                        c = PeekChar();
                        if (c == '-' || c == '+')
                            mStringBuilder.Append(NextChar());
                    }
                    else
                        break;
                }
                if (dotAppeared || exponentAppeared)
                {
                    if (!double.TryParse(mStringBuilder.ToString(), System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out tokenData.FloatValue))
                        throw new ParserException($"Invalid number '{mStringBuilder}' in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                    tokenData.Type = Token.NumberFloat;
                }
                else if (minusAppeared)
                {
                    if (!long.TryParse(mStringBuilder.ToString(), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out tokenData.IntegerValue))
                        throw new ParserException($"Invalid number '{mStringBuilder}' in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                    tokenData.Type = Token.NumberInteger;
                }
                else
                {
                    if (!ulong.TryParse(mStringBuilder.ToString(), System.Globalization.NumberStyles.Integer, CultureInfo.InvariantCulture, out tokenData.UnsignedIntegerValue))
                        throw new ParserException($"Invalid number '{mStringBuilder}' in line {lineNo} at position {columnIndex}.", lineNo, columnIndex);
                    tokenData.Type = Token.NumberUnsignedInteger;
                }
            }
        }

        #endregion
    }
}
