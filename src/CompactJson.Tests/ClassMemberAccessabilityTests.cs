using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;

namespace CompactJson.Tests
{
    [TestFixture]
    public class ClassMemberAccessabilityTests
    {
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

            public string PrivateGetter { private get; set; }

            private string PrivateProperty { get; set; }

            private string PrivateField;

            [JsonProperty]
            private readonly string PrivateReadOnlyField;
        }

        [Test]
        public void Parsing_into_readonly_field_should_fail()
        {
            Assert.That(() => Serializer.Parse<TestClassPrivateMembers>("{ \"PrivateReadOnlyField\": \"123\" }"), Throws.Exception);
        }
    }
}
