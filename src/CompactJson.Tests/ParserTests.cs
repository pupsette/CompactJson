using NUnit.Framework;

namespace CompactJson.Tests
{
    [TestFixture]
    public partial class ParserTests
    {
        // empty objects
        [TestCase("{}", "{}")]
        [TestCase("  {}", "{}")]
        [TestCase("{}   ", "{}")]
        [TestCase("{   }\t", "{}")]
        [TestCase("  { \t  }   ", "{}")]
        [TestCase("\r\n  {\n\t  } \r\n", "{}")]

        // null
        [TestCase("\r\n null\r\n", "N")]
        [TestCase("null", "N")]

        // arrays with mixed content
        [TestCase("\t[null]", "[N]")]
        [TestCase("\t[null  ,null]", "[NN]")]
        [TestCase("[1,null]", "[L1N]")]
        [TestCase("[  +1, null, true]", "[L1NT]")]
        [TestCase("[  +1,false,false]", "[L1FF]")]
        [TestCase("[{},-1, null]", "[{}L-1N]")]
        [TestCase("[[],[],[null]]", "[[][][N]]")]
        [TestCase("[[[-1,1,+1]]]", "[[[L-1L1L1]]]")]

        // boolean values
        [TestCase("\ttrue\r\n ", "T")]
        [TestCase("true", "T")]
        [TestCase("\tfalse \t", "F")]
        [TestCase("false", "F")]

        // floating point values and integers
        [TestCase("\t+5.125\r\n ", "D5.125")]
        [TestCase("-6.2", "D-6.2")]
        [TestCase("-9127361", "L-9127361")]
        [TestCase("+0", "L0")]
        [TestCase("-0", "L0")]
        [TestCase("+0.0", "D0")]
        [TestCase("-0.0", "D0")]
        [TestCase("-10.0", "D-10")]
        [TestCase("-10.0E2", "D-1000")]
        [TestCase("-5.0E-2", "D-0.05")]
        [TestCase("-5.0E+2", "D-500")]
        [TestCase("-5E+2", "D-500")]
        [TestCase("5E-2", "D0.05")]
        [TestCase("+251", "L251")]
        [TestCase("2938123", "L2938123")]
        [TestCase("0", "L0")]
        [TestCase("0.0", "D0")]
        [TestCase("      002", "L2")]
        [TestCase("\r\n-0\n", "L0")]
        [TestCase("  +0.0  ", "D0")]
        [TestCase("\t-0.0 ", "D0")]
        [TestCase("+251   ", "L251")]
        [TestCase("2938123  ", "L2938123")]
        [TestCase("0 \n", "L0")]
        [TestCase(" \t  0.0", "D0")]

        // strings
        [TestCase("\t\"abc\" ", "abc")]
        [TestCase("\"912\"", "912")]
        [TestCase("\r\n\"1\"  \t", "1")]
        [TestCase("\" \\n \"", " \n ")]
        [TestCase("\" \\t \\n \"", " \t \n ")]
        [TestCase("\" \\f \\n \"", " \f \n ")]
        [TestCase("\" \\r \\n \"", " \r \n ")]
        [TestCase("\" \\b \\n \"", " \b \n ")]
        [TestCase("\" \\/ / \"", " / / ")]
        [TestCase("\" \\\\b \"", " \\b ")]
        [TestCase("\"\\\\\\b \"", "\\\b ")]
        [TestCase("\"\\\" \\\"\"", "\" \"")]
        [TestCase("\" \\u000A \\n \"", " \n \n ")]
        [TestCase("\" \\u000a \\n \"", " \n \n ")]
        [TestCase("\" \\u1337 \\n \"", " \u1337 \n ")]

        // objects
        [TestCase("{\"P1\"  :  1}", "{P1:L1}")]
        [TestCase("{\"P1\"  :  [1, 2],\"P2\":false}", "{P1:[L1L2]P2:F}")]
        [TestCase("{\r\n  \"P1\"  :  [1, {}],\"P2\":false}", "{P1:[L1{}]P2:F}")]
        [TestCase("{ \t \"P1\" \n : \n [1, \n {\"P1\" :\"ST\\\"R1\"}],\"P2\":false \r\n}", "{P1:[L1{P1:ST\"R1}]P2:F}")]
        [TestCase("{ \r\n \"  \" : \nnull }", "{  :N}")]
        public void Parse(string input, string expectedConsumerCalls)
        {
            JsonTestConsumer consumer = new JsonTestConsumer();
            Serializer.Parse(input, consumer);
            Assert.That(consumer.ToString(), Is.EqualTo(expectedConsumerCalls));
        }

        [TestCase("")]
        [TestCase("null null")]
        [TestCase("\"asd ")]
        [TestCase("  98\" ")]
        [TestCase("  9.9.9 ")]
        [TestCase("  9E2E1 ")]
        [TestCase("  9E+-2 ")]
        [TestCase("  9ee-2 ")]
        [TestCase("  9 e-2 ")]
        [TestCase("  -+9 ")]
        [TestCase(" -9,8 ")]
        [TestCase("9a")]
        [TestCase("{5}")]
        [TestCase("[1,]")]
        [TestCase("[5,()]")]
        [TestCase(" {")]
        [TestCase(" [}")]
        [TestCase(" [123, 12")]
        [TestCase(" [123, 12}")]
        [TestCase(" false2 ")]
        [TestCase(" true_ ")]
        [TestCase(" TRUE ")]
        [TestCase(" true false ")]
        [TestCase(" trueMan ")]
        [TestCase(" 1 4 ")]
        [TestCase("\"\n\"")]
        [TestCase("\" \\o \"")]
        [TestCase(" {8:1}")]
        [TestCase(" {A:1}")]
        [TestCase(" {\"A\":}")]
        [TestCase(" {\"A\" 1:}")]
        [TestCase(" {\"A\",\"1\":true}")]
        public void Parsing_Should_Fail(string input)
        {
            JsonTestConsumer consumer = new JsonTestConsumer();
            TestDelegate parseAction = () => Serializer.Parse(input, consumer);
            Assert.That(parseAction, Throws.Exception);
        }
    }
}
