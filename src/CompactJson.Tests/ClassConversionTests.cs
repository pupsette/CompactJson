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
        public void Conversion_to_model_and_back(string input, Type type, string expectedOutput)
        {
            object model = Serializer.Parse(input, type);
            string output = Serializer.ToString(model, false);
            Assert.That(output, Is.EqualTo(expectedOutput));
        }
    }
}
