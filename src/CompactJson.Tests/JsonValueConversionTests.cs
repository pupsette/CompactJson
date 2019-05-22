using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CompactJson.Tests
{
    [TestFixture]
    public class JsonValueConversionTests
    {
        // booleans
        [TestCase("true", typeof(bool), "true")]
        [TestCase(" false ", typeof(bool), "false")]

        // numbers
        [TestCase("9192", typeof(long), "9192")]
        [TestCase("9192", typeof(int), "9192")]
        [TestCase("9192.125", typeof(double), "9192.125")]
        [TestCase("9192.125E+1", typeof(double), "91921.25")]
        [TestCase("9192.125E+1", typeof(float), "91921.25")]

        // nullables
        [TestCase("null", typeof(int?), "null")]
        [TestCase("158", typeof(int?), "158")]
        [TestCase("null", typeof(double?), "null")]
        [TestCase("158", typeof(double?), "158")]
        [TestCase("null", typeof(DateTime?), "null")]
        [TestCase("\"2001-05-13T00:12:22\"", typeof(DateTime?), "\"2001-05-13T00:12:22\"")]

        // datetime
        [TestCase("\"2001-05-13T00:12:22\"", typeof(DateTime), "\"2001-05-13T00:12:22\"")]
        [TestCase("\"2001-05-13T00:12:22Z\"", typeof(DateTime), "\"2001-05-13T00:12:22Z\"")]

        // strings
        [TestCase("null", typeof(string), "null")]
        [TestCase("\"!!!\" ", typeof(string), "\"!!!\"")]
        [TestCase("\"!\\n!\" ", typeof(string), "\"!\\n!\"")]

        // arrays
        [TestCase("null", typeof(int[]), "null")]
        [TestCase(" [1,6,2] ", typeof(int[]), "[1,6,2]")]
        [TestCase(" [1, 6.0,2] ", typeof(double[]), "[1,6,2]")]
        [TestCase(" [1, 6.0,2] ", typeof(List<double>), "[1,6,2]")]
        [TestCase(" [\"hi\", 6.0, 2] ", typeof(List<object>), "[\"hi\",6,2]")]
        public void Conversion_to_model_and_back(string input, Type type, string expectedOutput)
        {
            JsonValue jsonValue = Serializer.Parse(input);
            Assert.That(jsonValue.ToString(false), Is.EqualTo(expectedOutput));

            object model = jsonValue.ToModel(type);
            string output = Serializer.ToString(model, false);

            Assert.That(output, Is.EqualTo(expectedOutput));
        }
    }
}
