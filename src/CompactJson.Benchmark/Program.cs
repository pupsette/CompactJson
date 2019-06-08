using CompactJson.Benchmark.TestData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CompactJson.Benchmark
{

    class Program
    {
        private interface ISerializer
        {
            void SerializeToStream(List<Book> data, TextWriter textWriter);
            List<Book> DeserializeFromStream(TextReader textReader);
        }

        private static void PlottedTestRun(int numberOfListItems, int numberOfRuns, ISerializer serializer)
        {
            TimeSpan[] timeSpans = TestRun(numberOfListItems, numberOfRuns, serializer, out long charactersWritten);

            Console.WriteLine($"List with {numberOfListItems} items {numberOfRuns} times (~{charactersWritten / 1000 / 1000.0:0.##}MB):");
            Console.WriteLine($"Serialization: {timeSpans[0]}");
            Console.WriteLine($"Deserialization: {timeSpans[1]}");
        }

        private static TimeSpan[] TestRun(int numberOfListItems, int numberOfRuns, ISerializer serializer, out long charactersWritten)
        {
            // generate the object model
            var random = new Random(2919);
            var list = new List<Book>(numberOfListItems);
            for (int i = 0; i < numberOfListItems; i++)
                list.Add(Book.BOOKS[random.Next(Book.BOOKS.Length)]);

            // serialize the data
            Stopwatch stopwatch = new Stopwatch();
            StringWriter stringWriter = null;
            for (int i = 0; i < numberOfRuns; i++)
            {
                stringWriter = new StringWriter();
                stopwatch.Start();
                serializer.SerializeToStream(list, stringWriter);
                stopwatch.Stop();
            }
            TimeSpan serializationTime = stopwatch.Elapsed;

            string json = stringWriter.ToString();
            charactersWritten = json.Length * numberOfRuns;

            // deserialize the data
            stopwatch.Reset();
            for (int i = 0; i < numberOfRuns; i++)
            {
                StringReader stringReader = new StringReader(json);
                stopwatch.Start();
                List<Book> tmpList = serializer.DeserializeFromStream(stringReader);
                stopwatch.Stop();
                if (tmpList.Count != list.Count)
                    throw new Exception("Serializer was expected to deserialize as many items.");
            }
            TimeSpan deserializationTime = stopwatch.Elapsed;

            return new TimeSpan[] { serializationTime, deserializationTime };
        }

        private class NewtonsoftJsonSerializer : ISerializer
        {
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer
            {
                Formatting = Newtonsoft.Json.Formatting.None
            };

            public List<Book> DeserializeFromStream(TextReader textReader)
            {
                using (var jsonReader = new Newtonsoft.Json.JsonTextReader(textReader))
                    return serializer.Deserialize<List<Book>>(jsonReader);
            }

            public void SerializeToStream(List<Book> data, TextWriter textWriter)
            {
                serializer.Serialize(textWriter, data);
            }
        }

        private class CompactJsonSerializer : ISerializer
        {
            public List<Book> DeserializeFromStream(TextReader textReader)
            {
                return CompactJson.Serializer.Parse<List<Book>>(textReader);
            }

            public void SerializeToStream(List<Book> data, TextWriter textWriter)
            {
                CompactJson.Serializer.Write(data, textWriter, false);
            }
        }

        private static readonly Dictionary<string, ISerializer> TESTABLE_SERIALIZERS = new Dictionary<string, ISerializer>
        {
            { "Json.NET", new NewtonsoftJsonSerializer() },
            { "CompactJson", new CompactJsonSerializer() }
        };

        static int Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Choose a serializer to test: " + string.Join(", ", TESTABLE_SERIALIZERS.Keys));
                    return -1;
                }

                if (!TESTABLE_SERIALIZERS.TryGetValue(args[0], out ISerializer serializer))
                {
                    Console.WriteLine($"Unknown serializer '{args[0]}' choose one of: " + string.Join(", ", TESTABLE_SERIALIZERS.Keys));
                    return -1;
                }

                // Testing JIT compilation and warm-up time.
                TimeSpan[] timeSpans = TestRun(1, 1, serializer, out _);
                Console.WriteLine($"Warm-up: {timeSpans[0] + timeSpans[1]}");

                // small set
                PlottedTestRun(5, 200000, serializer);

                // large set
                PlottedTestRun(50000, 25, serializer);

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }
        }
    }
}
