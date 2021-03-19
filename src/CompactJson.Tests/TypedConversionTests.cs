using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CompactJson.Tests
{
    [TestFixture]
    public class TypedConversionTests
    {
        [JsonTypeName(typeof(DerivedDefaultClass), null)]
        [JsonTypeName(typeof(DerivedC), "C")]
        [JsonCustomConverter(typeof(TypedConverterFactory), "TT")]
        public abstract class AbstractBaseClassWithDefault
        {
            public string TheBase;
        }

        public class DerivedDefaultClass : AbstractBaseClassWithDefault
        {
            public bool Default;
        }

        public class DerivedC : AbstractBaseClassWithDefault
        {
            public string B;
        }

        [JsonTypeName(typeof(BaseClass), "BASE")]
        [JsonTypeName(typeof(DerivedA), "A")]
        [JsonTypeName(typeof(DerivedB), "B")]
        [JsonCustomConverter(typeof(TypedConverterFactory), "_typilo_")]
        public class BaseClass
        {
            public string TheBase;
        }

        public class DerivedA : BaseClass
        {
            public int A;

            [JsonProperty("Red")]
            public string AlwaysPresent = "HELLO";
        }

        public class DerivedB : BaseClass
        {
            public bool? B;

            [JsonIgnoreMember]
            public string AlwaysPresent = "HELLO";
        }

        public class Container
        {
            public List<BaseClass> Objects;
        }

        [Test]
        public void Serialize_and_deserialize_type_information()
        {
            var container = new Container();
            container.Objects = new List<BaseClass>();

            container.Objects.Add(new DerivedB { TheBase = "Look", B = false });
            container.Objects.Add(new BaseClass { TheBase = "" });
            container.Objects.Add(new BaseClass { });
            container.Objects.Add(new DerivedA { A = 19 });

            string json = Serializer.ToString(container, true);
            Console.WriteLine(json);
            Serializer.Parse<Container>(json);

            Assert.That(container.Objects.Count, Is.EqualTo(4));
            Assert.That(container.Objects[0], Is.TypeOf<DerivedB>());
            Assert.That(container.Objects[1], Is.TypeOf<BaseClass>());
            Assert.That(container.Objects[2], Is.TypeOf<BaseClass>());
            Assert.That(container.Objects[3], Is.TypeOf<DerivedA>());
        }

        [Test]
        public void Dont_serialize_and_deserialize_type_name_for_default()
        {
            string json = Serializer.ToString(new DerivedDefaultClass { TheBase = "Base", Default = true }, false);
            Assert.That(json, Is.EqualTo("{\"Default\":true,\"TheBase\":\"Base\"}"));
            AbstractBaseClassWithDefault result = Serializer.Parse<AbstractBaseClassWithDefault>(json);
            Assert.That(result.TheBase, Is.EqualTo("Base"));
            Assert.That(result, Is.InstanceOf<DerivedDefaultClass>());
            Assert.That(((DerivedDefaultClass)result).Default, Is.True);
        }
    }
}
