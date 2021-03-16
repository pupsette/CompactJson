using NUnit.Framework;
using System;

namespace CompactJson.Tests
{
    [TestFixture]
    public class CustomConverterTests
    {
        private class HexNumberConverter : IConverter
        {
            public Type Type => null;

            public IJsonArrayConsumer FromArray(Action<object> whenDone)
            {
                throw new NotImplementedException();
            }

            public object FromBoolean(bool value)
            {
                throw new NotImplementedException();
            }

            public object FromNull()
            {
                throw new NotImplementedException();
            }

            public object FromNumber(double value)
            {
                throw new NotImplementedException();
            }

            public object FromNumber(long value)
            {
                throw new NotImplementedException();
            }

            public object FromNumber(ulong value)
            {
                throw new NotImplementedException();
            }

            public IJsonObjectConsumer FromObject(Action<object> whenDone)
            {
                throw new NotImplementedException();
            }

            public object FromString(string value)
            {
                return int.Parse(value, System.Globalization.NumberStyles.HexNumber);
            }

            public void Write(object value, IJsonConsumer writer)
            {
                writer.String(((int)value).ToString("X"));
            }
        }

        // test classes
        private class TestClass
        {
            [CustomConverter(typeof(HexNumberConverter))]
            public int Number { get; set; }
        }

        [TestCase("{ \"Number\": \"5\" }", 5, "{\"Number\":\"5\"}")]
        [TestCase("{ \"Number\": \"a0\" }", 160, "{\"Number\":\"A0\"}")]
        [TestCase("{ \"Number\": \"FAe\" }", 4014, "{\"Number\":\"FAE\"}")]
        public void Conversion_to_model_and_back(string input, int expectedNumber, string expectedOutput)
        {
            TestClass model = Serializer.Parse<TestClass>(input);
            Assert.That(model.Number, Is.EqualTo(expectedNumber));
            string output = Serializer.ToString(model, false);
            Assert.That(output, Is.EqualTo(expectedOutput));
        }
    }
}
