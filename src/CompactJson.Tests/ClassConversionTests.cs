using NUnit.Framework;
using System;

namespace CompactJson.Tests
{
    [TestFixture]
    public class ClassConversionTests
    {
        // test classes
        private class TestClass
        {
            public int Number { get; set; }
        }

        private class TestClassEmitDefault : TestClass
        {
            [EmitDefaultValue]
            public int DefNumber { get; set; }
        }

        private class TestClassEmitNullableDefault : TestClass
        {
            [EmitDefaultValue]
            public int? DefNumber { get; set; }
        }

        private class TestClassNested : TestClass
        {
            public TestClass Nested { get; set; }
        }

        private class TestClassIgnoredMember
        {
            [JsonIgnoreMember]
            public string Ignored { get; set; } = "Ignored";

            public string Valid { get; set; } = "Valid";
        }

        [TestCase("{ \"Number\": 5 }", typeof(TestClass), "{\"Number\":5}")]
        [TestCase("{ \"Number\": 0 }", typeof(TestClass), "{}")]
        [TestCase("{ \"Number\": 0, \"OtherProperty\": 109 }", typeof(TestClass), "{}")]
        [TestCase("{ \"Number\": 3, \"OtherProperty\": 109 }", typeof(TestClass), "{\"Number\":3}")]
        [TestCase("{ \"DefNumber\": 5 }", typeof(TestClassEmitDefault), "{\"DefNumber\":5}")]
        [TestCase("{ \"DefNumber\": 0 }", typeof(TestClassEmitDefault), "{\"DefNumber\":0}")]
        [TestCase("{ \"DefNumber\": 5, \"Number\": 0 }", typeof(TestClassEmitNullableDefault), "{\"DefNumber\":5}")]
        [TestCase("{ \"DefNumber\": null, \"Number\": 0 }", typeof(TestClassEmitNullableDefault), "{\"DefNumber\":null}")]
        [TestCase("{ \"DefNumber\": 0, \"Number\": 0 }", typeof(TestClassEmitNullableDefault), "{\"DefNumber\":0}")]
        [TestCase("{ \"Number\": 0 }", typeof(TestClassEmitNullableDefault), "{\"DefNumber\":null}")]
        [TestCase("{ \"Nested\": null }", typeof(TestClassNested), "{}")]
        [TestCase("{ \"Nested\": {} }", typeof(TestClassNested), "{\"Nested\":{}}")]
        [TestCase("{ \"Nested\": {\"Number\":  190} }", typeof(TestClassNested), "{\"Nested\":{\"Number\":190}}")]
        [TestCase("{}", typeof(TestClassNested), "{}")]
        [TestCase("null", typeof(TestClassNested), "null")]
        [TestCase("{}", typeof(TestClassIgnoredMember), "{\"Valid\":\"Valid\"}")]
        [TestCase("{}", typeof(TestClassEmbeddedJsonValue), "{\"TestValue\":33}")]
        [TestCase("{\"Anything\":null}", typeof(TestClassEmbeddedJsonValue), "{\"Anything\":null,\"TestValue\":33}")]
        [TestCase("{\"Anything\":[1]}", typeof(TestClassEmbeddedJsonValue), "{\"Anything\":[1],\"TestValue\":33}")]
        [TestCase("{\"Anything\":{}}", typeof(TestClassEmbeddedJsonValue), "{\"Anything\":{},\"TestValue\":33}")]
        [TestCase("{\"Anything\":true}", typeof(TestClassEmbeddedJsonValue), "{\"Anything\":true,\"TestValue\":33}")]
        [TestCase("{\"NotAnything\":{}}", typeof(TestClassEmbeddedJsonObject), "{\"NotAnything\":{},\"TestValue\":33}")]
        [TestCase("{\"NotAnything\":null}", typeof(TestClassEmbeddedJsonObject), "{\"TestValue\":33}")]
        [TestCase("{}", typeof(TestClassEmbeddedJsonObject), "{\"TestValue\":33}")]
        public void Conversion_to_model_and_back(string input, Type type, string expectedOutput)
        {
            object model = Serializer.Parse(input, type);
            string output = Serializer.ToString(model, false);
            Assert.That(output, Is.EqualTo(expectedOutput));
        }

        [TestCase("{\"NotAnything\":\"\"}", typeof(TestClassEmbeddedJsonObject))]
        public void Conversion_to_model_should_fail(string input, Type type)
        {
            Exception error = null;
            try
            {
                Serializer.Parse(input, type);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                error = ex;
            }
            Assert.That(error, Is.Not.Null);
        }

        private class TestClassEmbeddedJsonValue
        {
            public JsonValue Anything { get; set; }

            public int TestValue { get; set; } = 33;
        }

        private class TestClassEmbeddedJsonObject
        {
            public JsonObject NotAnything { get; set; }

            public int TestValue { get; set; } = 33;
        }
    }
}
