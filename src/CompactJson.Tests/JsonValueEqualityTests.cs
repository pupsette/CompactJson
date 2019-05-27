using NUnit.Framework;

namespace CompactJson.Tests
{
    [TestFixture]
    public class JsonValueEqualityTests
    {
        [TestCase("{ \"prop1\" : 500, \"prop2\": 399}", "{ \"prop1\" : 500, \"prop2\": 399}")]
        [TestCase("true", "true")]
        [TestCase("null", "null ")]
        [TestCase(" [1,6,2] ", "[1,6.0,2]")]
        [TestCase(" [\"hi\", 6.0, 2] ", "[\"hi\",6.0,2]")]
        public void Parse_to_generic_model_and_compare_for_equality(string input1, string input2)
        {
            JsonValue jsonValue1 = Serializer.Parse(input1);
            JsonValue jsonValue2 = Serializer.Parse(input2 ?? input1);

            Assert.That(jsonValue1.Equals(jsonValue2));
            Assert.That(jsonValue1.Equals(jsonValue1));

            Assert.That(jsonValue1.GetHashCode(), Is.EqualTo(jsonValue2.GetHashCode()));
        }

        [Test]
        public void JsonNull_should_differ_from_null()
        {
            Assert.That(new JsonNull().Equals(null), Is.False);
        }

        [Test]
        public void Equals_method_should_not_throw_when_passing_null()
        {
            Assert.That(new JsonArray().Equals(null), Is.False);
        }

        [Test]
        public void Equals_method_should_not_throw_when_passing_different_types()
        {
            Assert.That(new JsonArray().Equals(15), Is.False);
            Assert.That(new JsonArray().Equals("15"), Is.False);
            Assert.That(new JsonArray().Equals(new object[0]), Is.False);
        }

        [TestCase("{ \"prop1\" : 500, \"prop2\": 399}", "{ \"prop2\" : 399, \"prop1\": 500}")]
        [TestCase("{\"1\": 5}", "[\"1\", 5]")]
        [TestCase("{}", "[]")]
        [TestCase("[1,2]", "[2,1]")]
        [TestCase("null", "\"null\"")]
        [TestCase("null", "0")]
        public void Parse_to_generic_model_and_compare_for_inequality(string input1, string input2)
        {
            JsonValue jsonValue1 = Serializer.Parse(input1);
            JsonValue jsonValue2 = Serializer.Parse(input2);

            // of course, there is no guarantee for no collision!
            // still, the tests will ensure that there are no collisions for our test input.
            Assert.That(jsonValue1.GetHashCode(), Is.Not.EqualTo(jsonValue2.GetHashCode()));

            Assert.That(jsonValue1.Equals(jsonValue2), Is.False);
        }
    }
}
