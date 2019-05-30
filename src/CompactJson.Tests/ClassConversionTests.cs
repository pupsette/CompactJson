using NUnit.Framework;
using NUnit.Framework.Constraints;
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

        private class TestClassPrivateMembers
        {
            public TestClassPrivateMembers()
            {
            }

            public TestClassPrivateMembers(string privateSetterPropertyValue, string privatePropertyValue, string privateFieldValue, string privateReadonlyFieldValue)
            {
                PrivateSetter = privateSetterPropertyValue;
                PrivateProperty = privatePropertyValue;
                PrivateField = privateFieldValue;
                PrivateReadOnlyField = privateReadonlyFieldValue;
            }

            internal readonly string writeOk;
            public string WriteOk { get { return writeOk; } }

            public string PrivateSetter { get; private set; }

            private string PrivateProperty { get; set; }

            private string PrivateField;

            private readonly string PrivateReadOnlyField;
        }

        private class TestBaseClass
        {
            public int Base { get; set; }
        }

        private class TestClassDerivedA : TestBaseClass
        {
            public int ValueA { get; set; }
        }

        private class TestClassDerivedB : TestBaseClass
        {
            public int ValueB { get; set; }
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
        public void Conversion_to_model_and_back(string input, Type type, string expectedOutput)
        {
            object model = Serializer.Parse(input, type);
            string output = Serializer.ToString(model, false);
            Assert.That(output, Is.EqualTo(expectedOutput));
        }

        //[TestCase("{ \"Int\": 16 }", typeof(int), "{\"Int\":16}")]
        //[TestCase("{ \"String\": \"123\" }", typeof(string), "{\"String\":\"123\"}")]
        //[TestCase("{ \"String\": null }", null, "{}")]
        //[TestCase("{ \"Float\": 15 }", typeof(double), "{\"Float\":15}")]
        //[TestCase("{ \"IntArray\": [1,2,3] }", typeof(int[]), "{\"IntArray\":[1,2,3]}")]
        //[TestCase("{}", null, "{}")]
        //public void Conversion_to_model_with_multi_property_of_type_object(string input, Type expectedPropertyType, string expectedOutput)
        //{
        //    TestClassMulti model = Serializer.Parse<TestClassMulti>(input);

        //    Assert.That(model.Multi, expectedPropertyType != null ? (IResolveConstraint)Is.InstanceOf(expectedPropertyType) : Is.Null);
        //    string output = Serializer.ToString(model, false);
        //    Assert.That(output, Is.EqualTo(expectedOutput));
        //}

        //[TestCase("{}", null, "{}")]
        //[TestCase("{\"A\":{}}", typeof(TestClassDerivedA), "{\"A\":{}}")]
        //[TestCase("{\"A\":{\"ValueB\":100}}", typeof(TestClassDerivedA), "{\"A\":{}}")]
        //[TestCase("{\"B\":{\"ValueB\":100}}", typeof(TestClassDerivedB), "{\"B\":{\"ValueB\":100}}")]
        //public void Conversion_to_model_with_multi_property_of_derived_type(string input, Type expectedPropertyType, string expectedOutput)
        //{
        //    TestClassMulti model = Serializer.Parse<TestClassMulti>(input);

        //    Assert.That(model.MultiDerived, expectedPropertyType != null ? (IResolveConstraint)Is.InstanceOf(expectedPropertyType) : Is.Null);
        //    string output = Serializer.ToString(model, false);
        //    Assert.That(output, Is.EqualTo(expectedOutput));
        //}

        //// invalid test class
        //private class TestClassInvalidMultiType1
        //{
        //    [MultiTypeProperty("A", typeof(int))]
        //    [MultiTypeProperty("A", typeof(double))]
        //    public object Multi { get; set; }
        //}

        //[TestCase(typeof(TestClassInvalidMultiType1))]
        //public void Invalid_MultiTypeProperty_should_throw(Type modelType)
        //{
        //    Assert.That(() => Serializer.Parse("{}", modelType), Throws.Exception);
        //}

    }
}
