using System;

namespace CompactJson.Benchmark.TestData
{
    class Author
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public DateTime? Birth { get; set; }

        public double PopularityScore { get; set; }

        public static readonly Author[] AUTHORS = new[]
        {
            new Author
            {
                FirstName = "Peter",
                LastName = "Miller",
                Birth = new DateTime(1980, 05, 12),
                PopularityScore = 9.2124
            },
            new Author
            {
                FirstName = "Paul",
                LastName = "Smith",
                Birth = new DateTime(1955, 02, 20),
                PopularityScore = 11.2
            },
            new Author
            {
                FirstName = "Mary",
                LastName = "Mason",
                PopularityScore = 0.691
            }
        };
    }
}
