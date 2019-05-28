using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace CompactJson.Tests
{
    [TestFixture]
    public class TypedConversionTests
    {
        [TypeName(typeof(BaseClass), "BASE")]
        [TypeName(typeof(DerivedA), "A")]
        [TypeName(typeof(DerivedB), "B")]
        [CustomConverter(typeof(TypedConverterFactory), "_typilo_")]
        public class BaseClass
        {
            public string TheBase;
        }

        public class DerivedA : BaseClass
        {
            public int A;
        }

        public class DerivedB : BaseClass
        {
            public bool? B;
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
    }
}
