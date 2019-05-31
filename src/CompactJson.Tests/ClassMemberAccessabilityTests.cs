using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;

namespace CompactJson.Tests
{
    [TestFixture]
    public class ClassMemberAccessabilityTests
    {
        private class TestClass
        {
            public TestClass()
            {
            }

            public TestClass(string privateSetterPropertyValue, string privatePropertyValue, string privateReadonlyFieldValue)
            {
                PrivateSetter = privateSetterPropertyValue;
                PrivateProperty = privatePropertyValue;
                PrivateReadOnlyField = privateReadonlyFieldValue;
            }

            internal readonly string writeOk;
            public string WriteOk { get { return writeOk; } }

            public string PrivateSetter { get; private set; }

            public string PrivateGetter { private get; set; }

            private string PrivateProperty { get; set; }

            [JsonProperty]
            public readonly string PrivateReadOnlyField;
        }

        [Test]
        public void Parsing_into_readonly_field_should_fail()
        {
            Assert.That(() => Serializer.Parse<TestClass>("{ \"PrivateReadOnlyField\": \"123\" }"), Throws.Exception);
        }

        private class PrivateFields
        {
            [JsonProperty]
            private string PrivateFieldWithAttribute = "333";

            private string PrivateFieldWithoutAttribute = "456";

            public string GetPrivateFieldWithoutAttribute() { return PrivateFieldWithoutAttribute; }

            public string GetPrivateFieldWithAttribute() { return PrivateFieldWithAttribute; }
        }

        [Test]
        public void Private_field_should_be_ignored_when_not_attributed()
        {
            PrivateFields obj = Serializer.Parse<PrivateFields>("{ \"PrivateFieldWithoutAttribute\": \"123\" }");
            Assert.That(obj.GetPrivateFieldWithoutAttribute(), Is.EqualTo("456"));
        }

        [Test]
        public void Private_field_should_be_parsed_when_attributed()
        {
            PrivateFields obj = Serializer.Parse<PrivateFields>("{ \"PrivateFieldWithAttribute\": \"123\" }");
            Assert.That(obj.GetPrivateFieldWithAttribute(), Is.EqualTo("123"));
        }

        [Test]
        public void Private_fields_are_emitted_according_to_attributes()
        {
            string json = Serializer.ToString(new PrivateFields(), false);
            Assert.That(json, Is.EqualTo("{\"PrivateFieldWithAttribute\":\"333\"}"));
        }

        private class ProtectedFields
        {
            [JsonProperty]
            protected string ProtectedFieldWithAttribute = "333";

            protected string ProtectedFieldWithoutAttribute = "456";

            public string GetProtectedFieldWithoutAttribute() { return ProtectedFieldWithoutAttribute; }

            public string GetProtectedFieldWithAttribute() { return ProtectedFieldWithAttribute; }
        }

        [Test]
        public void Protected_field_should_be_ignored_when_not_attributed()
        {
            ProtectedFields obj = Serializer.Parse<ProtectedFields>("{ \"ProtectedFieldWithoutAttribute\": \"123\" }");
            Assert.That(obj.GetProtectedFieldWithoutAttribute(), Is.EqualTo("456"));
        }

        [Test]
        public void Protected_field_should_be_parsed_when_attributed()
        {
            ProtectedFields obj = Serializer.Parse<ProtectedFields>("{ \"ProtectedFieldWithAttribute\": \"123\" }");
            Assert.That(obj.GetProtectedFieldWithAttribute(), Is.EqualTo("123"));
        }

        [Test]
        public void Protected_fields_are_emitted_according_to_attributes()
        {
            string json = Serializer.ToString(new ProtectedFields(), false);
            Assert.That(json, Is.EqualTo("{\"ProtectedFieldWithAttribute\":\"333\"}"));
        }

        private class InternalFields
        {
            [JsonProperty]
            internal string InternalFieldWithAttribute = "333";

            internal string InternalFieldWithoutAttribute = "456";

            public string GetInternalFieldWithoutAttribute() { return InternalFieldWithoutAttribute; }

            public string GetInternalFieldWithAttribute() { return InternalFieldWithAttribute; }
        }

        [Test]
        public void Internal_field_should_be_ignored_when_not_attributed()
        {
            InternalFields obj = Serializer.Parse<InternalFields>("{ \"InternalFieldWithoutAttribute\": \"123\" }");
            Assert.That(obj.GetInternalFieldWithoutAttribute(), Is.EqualTo("456"));
        }

        [Test]
        public void Internal_field_should_be_parsed_when_attributed()
        {
            InternalFields obj = Serializer.Parse<InternalFields>("{ \"InternalFieldWithAttribute\": \"123\" }");
            Assert.That(obj.GetInternalFieldWithAttribute(), Is.EqualTo("123"));
        }

        [Test]
        public void Internal_fields_are_emitted_according_to_attributes()
        {
            string json = Serializer.ToString(new InternalFields(), false);
            Assert.That(json, Is.EqualTo("{\"InternalFieldWithAttribute\":\"333\"}"));
        }
    }
}
