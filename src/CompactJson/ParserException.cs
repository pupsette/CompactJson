using System;

namespace CompactJson
{
    /// <summary>
    /// A parser exception which adds line number and character position information.
    /// </summary>
#if COMPACTJSON_PUBLIC
    public
#else
    internal
#endif
    class ParserException : Exception
    {
        /// <summary>
        /// Initializes a new parser exception which contains line number and position
        /// information.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="lineNo">The line number.</param>
        /// <param name="position">The character position within the line.</param>
        /// <param name="ex">An optional inner exception.</param>
        public ParserException(string message, int lineNo, int position, Exception ex = null)
            : base(message, ex)
        {
            LineNumber = lineNo;
            Position = position;
        }

        /// <summary>
        /// The line number, where the error occurred. Line numbers start at 1.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// The character position within the line, where the error occurred. Character positions
        /// start at 1.
        /// </summary>
        public int Position { get; }
    }
}
