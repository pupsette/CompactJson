using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CompactJson.Tests
{
    [TestFixture]
    public class ConversionTests
    {
        // booleans
        [TestCase("true", typeof(bool), "true")]
        [TestCase(" false ", typeof(bool), "false")]

        // numbers
        [TestCase("9192", typeof(long), "9192")]
        [TestCase("9192", typeof(int), "9192")]
        [TestCase("9192.125", typeof(double), "9192.125")]
        [TestCase("9192.125E+1", typeof(double), "91921.25")]
        [TestCase("-88", typeof(double), "-88.0")]
        [TestCase("\"NaN\"", typeof(double), "\"NaN\"")]
        [TestCase("\"nan\"", typeof(double), "\"NaN\"")]
        [TestCase("\"+infinity\"", typeof(double), "\"Infinity\"")]
        [TestCase("\"-Infinity\"", typeof(double), "\"-Infinity\"")]
        [TestCase("9192.125E+1", typeof(float), "91921.25")]

        // nullables
        [TestCase("null", typeof(int?), "null")]
        [TestCase("158", typeof(int?), "158")]
        [TestCase("null", typeof(double?), "null")]
        [TestCase("158", typeof(double?), "158.0")]
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
        [TestCase(" [1, 6.0,2] ", typeof(double[]), "[1.0,6.0,2.0]")]
        [TestCase(" [1, 6.0,2] ", typeof(List<double>), "[1.0,6.0,2.0]")]
        [TestCase(" [\"hi\", 6.0, 2] ", typeof(List<object>), "[\"hi\",6.0,2]")]
        public void Conversion_to_model_and_back(string input, Type type, string expectedOutput)
        {
            object model = Serializer.Parse(input, type);
            string output = Serializer.ToString(model, false);
            Assert.That(output, Is.EqualTo(expectedOutput));
        }

        [TestCase("{}", typeof(string))]
        [TestCase("[]", typeof(string))]
        [TestCase("15", typeof(bool))]
        [TestCase("15", typeof(string))]
        [TestCase("\"asd\"", typeof(long))]
        [TestCase("9192.5", typeof(long))]
        [TestCase("NaN2", typeof(double))]
        public void Conversion_to_model_should_fail(string input, Type type)
        {
            Assert.That(() => Serializer.Parse(input, type), Throws.Exception);
        }

        [TestCase("\"2001-05-13T00:12:22+00:00\"")]
        [TestCase("\"2001-05-13T00:12:22+11:00\"")]
        [TestCase("\"2001-05-13T00:12:22+02:15\"")]
        [TestCase("\"2021-03-11T22:23:23.6396864+01:00\"", 640)]
        public void DateTime_with_offset_yields_correct_local_time(string input, int? expectedMillisecondsPart = null)
        {
            DateTime time = (DateTime)Serializer.Parse(input, typeof(DateTime));
            if (expectedMillisecondsPart.HasValue)
                Assert.That(time.Millisecond, Is.EqualTo(expectedMillisecondsPart.Value));
            Assert.That(time.Kind, Is.EqualTo(DateTimeKind.Local));
            TimeSpan offset = TimeZoneInfo.Local.GetUtcOffset(time);

            string outputTime = Serializer.ToString(time, false);
            Console.WriteLine(outputTime);

            Assert.That(outputTime, Does.EndWith("+" + offset.Hours.ToString("00") + ":" + offset.Minutes.ToString("00") + "\""));
        }

        [TestCase("\"2001-05-13T00:12:22.999\"", "\"2001-05-13T00:12:22.999\"")]
        [TestCase("\"2001-05-13T00:12:22.000\"", "\"2001-05-13T00:12:22\"")]
        [TestCase("\"2001-05-13T00:12:22.010\"", "\"2001-05-13T00:12:22.010\"")]
        [TestCase("\"2001-05-13T00:12:22.010Z\"", "\"2001-05-13T00:12:22.010Z\"")]
        [TestCase("\"2001-05-13T00:12:22.000Z\"", "\"2001-05-13T00:12:22Z\"")]
        public void DateTime_omit_milliseconds_on_serialization(string input, string expected)
        {
            DateTime time = (DateTime)Serializer.Parse(input, typeof(DateTime));

            string outputTime = Serializer.ToString(time, false);
            Assert.That(outputTime, Is.EqualTo(expected));
        }

        [TestCase("null", Description = "null is not allowed for DateTime. Must be explicitly nullable (DateTime?).")]
        [TestCase("\"2001-05-13T00:88:22\"")]
        [TestCase("\"52001-05-13T00:12:22\"")]
        [TestCase("\"2001-05-13T00:12:22.28b12\"")]
        [TestCase("\"2A01-05-13T00:12:22\"")]
        [TestCase("\"2001-05-13T00:12\"")]
        [TestCase("\"2001-05-13T00:12:0\"")]
        [TestCase("\"2001-05-13T00:12:00Y\"")]
        [TestCase("\"2001-13-13T00:12:00Z\"")]
        [TestCase("\"2001-10-32T10:12:00\"")]
        public void Invalid_DateTime_string_throws_exception_on_conversion(string input)
        {
            Exception ex = null;
            try
            {
                Serializer.Parse<DateTime>(input);
            }
            catch (Exception ee)
            {
                ex = ee;
                Console.WriteLine(ee.Message);
            }
            Assert.That(ex, Is.Not.Null);
        }
    }
}
