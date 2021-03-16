using NUnit.Framework;
using System;
using System.Text;

namespace CompactJson.Tests
{
    [TestFixture]
    public class ClassMemberAccessabilityTests
    {
        #region NON PUBLIC PROPERTIES

        private class NonPublicProperty
        {
            public string NonPublic1 { get; private set; } = "1";

            internal string GetNonPublic2() { return NonPublic2; }
            public string NonPublic2 { private get; set; } = "2";
            internal string NonPublic3 { get; set; } = "3";
            internal string GetNonPublic4() { return NonPublic4; }
            protected string NonPublic4 { get; set; } = "4";
            public string NonPublic5 { get; internal set; } = "5";
            public string NonPublic6 { internal get; set; } = "6";
            public string NonPublic7 { get; } = "7";
        }

        [Test]
        public void Non_public_properties_are_ignored()
        {
            StringBuilder sb = new StringBuilder("{");
            for (int i = 1; i <= 7; i++)
            {
                if (i > 1)
                    sb.Append(",");
                sb.Append("\"NonPublic");
                sb.Append(i);
                sb.Append("\":\"123\"");
            }
            sb.Append("}");
            Console.WriteLine(sb);
            NonPublicProperty obj = Serializer.Parse<NonPublicProperty>(sb.ToString());
            Assert.That(obj.NonPublic1, Is.EqualTo("123"));
            Assert.That(obj.GetNonPublic2(), Is.EqualTo("2"));
            Assert.That(obj.NonPublic3, Is.EqualTo("3"));
            Assert.That(obj.GetNonPublic4(), Is.EqualTo("4"));
            Assert.That(obj.NonPublic5, Is.EqualTo("123"));
            Assert.That(obj.NonPublic6, Is.EqualTo("6"));

            string json = Serializer.ToString(new NonPublicProperty(), false);
            Assert.That(json, Is.EqualTo("{\"NonPublic1\":\"1\",\"NonPublic5\":\"5\"}"));
        }

        private class NonPublicPropertyAttributed
        {
            [JsonProperty]
            public string NonPublic1 { get; private set; } = "1";

            [JsonProperty]
            public string NonPublic2 { private get; set; } = "2";

            [JsonProperty]
            internal string NonPublic3 { get; set; } = "3";

            [JsonProperty]
            protected string NonPublic4 { get; set; } = "4";

            [JsonProperty]
            public string NonPublic5 { get; internal set; } = "5";

            [JsonProperty]
            public string NonPublic6 { internal get; set; } = "6";

            [JsonProperty]
            public string NonPublic7 { get; } = "7";
        }

        [TestCase("NonPublic1")]
        [TestCase("NonPublic2")]
        [TestCase("NonPublic3")]
        [TestCase("NonPublic4")]
        [TestCase("NonPublic5")]
        [TestCase("NonPublic6")]
        public void Writing_and_reading_non_public_property(string propertyName)
        {
            string propertyPart = $"\"{propertyName}\":\"succeed\"";
            string json = "{ " + propertyPart + " }";
            NonPublicPropertyAttributed obj = Serializer.Parse<NonPublicPropertyAttributed>(json);

            string serializedJson = Serializer.ToString(obj, false);

            Assert.That(serializedJson.Contains(propertyPart), serializedJson);
        }

        [Test]
        public void Writing_non_public_property_without_setter_throws()
        {
            Assert.That(() => Serializer.Parse<NonPublicPropertyAttributed>("{\"NonPublic7\":\"fail\"}"), Throws.Exception);
        }

        [Test]
        public void Reading_non_public_property_without_setter_succeeds()
        {
            string json = Serializer.ToString(new NonPublicPropertyAttributed(), false);
            Assert.That(json.Contains("\"NonPublic7\":\"7\""), json);
        }

        #endregion

        #region PROPERTY WITHOUT GETTER

        #endregion

        #region READONLY_FIELDS

        private class ReadonlyFieldClass
        {
            [JsonProperty]
            public readonly string PrivateReadOnlyField = "90";
        }

        [Test]
        public void Parsing_into_readonly_field_should_fail()
        {
            Assert.That(() => Serializer.Parse<ReadonlyFieldClass>("{ \"PrivateReadOnlyField\": \"123\" }"), Throws.Exception);
        }

        [Test]
        public void Emitting_readonly_field_should_succeed()
        {
            string json = Serializer.ToString(new ReadonlyFieldClass(), false);
            Assert.That(json, Is.EqualTo("{\"PrivateReadOnlyField\":\"90\"}"));
        }

        #endregion

        #region PRIVATE FIELDS

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

        #endregion

        #region PROTECTED FIELDS

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

        #endregion

        #region INTERNAL FIELDS

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

        #endregion
    }
}
