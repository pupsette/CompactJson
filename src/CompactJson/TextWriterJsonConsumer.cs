using System.Globalization;
using System.IO;
using System.Text;

namespace CompactJson
{
    internal sealed class TextWriterJsonConsumer : IJsonConsumer
    {
        private readonly TextWriter mWriter;
        private readonly bool mIndentation;
        private readonly StringBuilder mTemp = new StringBuilder();

        private class TextWriterObjectJsonConsumer : IJsonObjectConsumer
        {
            private readonly TextWriterJsonConsumer mParent;
            private readonly string mPreviousIndentation;
            private string mCurrentIndentation;
            private bool mFirstProperty = true;

            public TextWriterObjectJsonConsumer(TextWriterJsonConsumer parent, string indentation)
            {
                this.mParent = parent;
                mPreviousIndentation = indentation;
            }

            public void PropertyName(string propertyName)
            {
                if (mFirstProperty)
                {
                    if (mPreviousIndentation != null)
                    {
                        mParent.mWriter.Write("{\n");
                        mCurrentIndentation = mPreviousIndentation + "  ";
                        mParent.mWriter.Write(mCurrentIndentation);
                    }
                    else
                        mParent.mWriter.Write("{");
                    mFirstProperty = false;
                }
                else
                {
                    if (mCurrentIndentation != null)
                    {
                        mParent.mWriter.Write(",\n");
                        mParent.mWriter.Write(mCurrentIndentation);
                    }
                    else
                        mParent.mWriter.Write(",");
                }
                mParent.String(propertyName);

                if (mCurrentIndentation != null)
                    mParent.mWriter.Write(": ");
                else
                    mParent.mWriter.Write(":");
            }

            public IJsonArrayConsumer Array()
            {
                return new TextWriterArrayJsonConsumer(mParent, mCurrentIndentation);
            }

            public void Boolean(bool value)
            {
                mParent.Boolean(value);
            }

            public void Done()
            {
                if (mFirstProperty)
                    mParent.mWriter.Write("{}");
                else
                {
                    if (mPreviousIndentation != null)
                    {
                        mParent.mWriter.Write('\n');
                        mParent.mWriter.Write(mPreviousIndentation);
                    }
                    mParent.mWriter.Write('}');
                }
            }

            public void Null()
            {
                mParent.Null();
            }

            public void Number(double value)
            {
                mParent.Number(value);
            }

            public void Number(long value)
            {
                mParent.Number(value);
            }

            public void Number(ulong value)
            {
                mParent.Number(value);
            }

            public IJsonObjectConsumer Object()
            {
                return new TextWriterObjectJsonConsumer(mParent,  mCurrentIndentation);
            }

            public void String(string value)
            {
                mParent.String(value);
            }
        }

        private class TextWriterArrayJsonConsumer : IJsonArrayConsumer
        {
            private readonly TextWriterJsonConsumer mParent;
            private readonly string mPreviousIndentation;
            private string mCurrentIndentation;
            private bool mFirstProperty = true;

            public TextWriterArrayJsonConsumer(TextWriterJsonConsumer parent, string indentation)
            {
                this.mParent = parent;
                mPreviousIndentation = indentation;
            }

            private void WriteItem()
            {
                if (mFirstProperty)
                {
                    if (mPreviousIndentation != null)
                    {
                        mParent.mWriter.Write("[\n");
                        mCurrentIndentation = mPreviousIndentation + "  ";
                        mParent.mWriter.Write(mCurrentIndentation);
                    }
                    else
                        mParent.mWriter.Write("[");
                    mFirstProperty = false;
                }
                else
                {
                    if (mCurrentIndentation != null)
                    {
                        mParent.mWriter.Write(",\n");
                        mParent.mWriter.Write(mCurrentIndentation);
                    }
                    else
                        mParent.mWriter.Write(",");
                }
            }

            public IJsonArrayConsumer Array()
            {
                WriteItem();
                return new TextWriterArrayJsonConsumer(mParent, mCurrentIndentation);
            }

            public void Boolean(bool value)
            {
                WriteItem();
                mParent.Boolean(value);
            }

            public void Done()
            {
                if (mFirstProperty)
                    mParent.mWriter.Write("[]");
                else
                {
                    if (mPreviousIndentation != null)
                    {
                        mParent.mWriter.Write('\n');
                        mParent.mWriter.Write(mPreviousIndentation);
                    }
                    mParent.mWriter.Write(']');
                }
            }

            public void Null()
            {
                WriteItem();
                mParent.Null();
            }

            public void Number(double value)
            {
                WriteItem();
                mParent.Number(value);
            }

            public void Number(long value)
            {
                WriteItem();
                mParent.Number(value);
            }

            public void Number(ulong value)
            {
                WriteItem();
                mParent.Number(value);
            }

            public IJsonObjectConsumer Object()
            {
                WriteItem();
                return new TextWriterObjectJsonConsumer(mParent, mCurrentIndentation);
            }

            public void String(string value)
            {
                WriteItem();
                mParent.String(value);
            }
        }

        public TextWriterJsonConsumer(TextWriter writer, bool indentation)
        {
            mWriter = writer;
            mIndentation = indentation;
        }

        public void Boolean(bool value)
        {
            mWriter.Write(value ? "true" : "false");
        }

        public void Null()
        {
            mWriter.Write("null");
        }

        public void Number(double value)
        {
            if ((value % 1) == 0)
                mWriter.Write(value.ToString("f1", CultureInfo.InvariantCulture));
            else
                mWriter.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        public void Number(long value)
        {
            mWriter.Write(value.ToString(CultureInfo.InvariantCulture));
        }
        public void Number(ulong value)
        {
            mWriter.Write(value.ToString(CultureInfo.InvariantCulture));
        }

        public void String(string value)
        {
            JsonString.Escape(value, mTemp);
            mWriter.Write(mTemp.ToString());
            mTemp.Clear();
        }

        public IJsonObjectConsumer Object()
        {
            return new TextWriterObjectJsonConsumer(this, mIndentation ? "" : null);
        }

        public IJsonArrayConsumer Array()
        {
            return new TextWriterArrayJsonConsumer(this, mIndentation ? "" : null);
        }
    }
}
