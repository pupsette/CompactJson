using System;
using System.Collections.Generic;

namespace CompactJson.Benchmark.TestData
{
    class Book
    {
        [JsonEmitNullValue]
        public string Title { get; set; }

        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public string Subtitle;

        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public DateTime? LastOrdered { get; set; }

        [JsonEmitNullValue]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public Category Category { get; set; }

        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public int? Pages { get; set; }

        [Newtonsoft.Json.JsonProperty(DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore)]
        public List<string> Chapters { get; set; }

        public DateTime Publication { get; set; }

        [JsonEmitNullValue]
        public Author[] Authors { get; set; }

        public static readonly Book[] BOOKS = new Book[]
        {
            new Book
            {
                Title = "Book Number 1",
                Subtitle = "Could also be number 2",
                Category = Category.Cookbook,
                Authors = new[] { Author.AUTHORS[0], Author.AUTHORS[1], Author.AUTHORS[2] },
                Publication = new DateTime(2018, 12, 21),
                Pages = 100,
                LastOrdered = new DateTime(2019, 1, 1, 11, 20, 39, 922, DateTimeKind.Utc),
                Chapters = new List<string>{ "Cake", "More Cake", "Sweets" }
            },
            new Book
            {
                Title = "Very old story",
                Category = Category.Crime,
                Authors = null,
                Publication = new DateTime(2010, 1, 1),
                Pages = 500,
                LastOrdered = new DateTime(2019, 1, 1, 11, 20, 39, 922, DateTimeKind.Local)
            },
            new Book
            {
                Title = "Long book title, much longer than a normal book, but still not too long for the JSON parser to struggle.",
                Subtitle = "There is a formatted subtitle:\n\t * Includes a bullet list\n\t * Contains no information\n\t * Uses tabs, newlines and '\\'",
                Category = Category.Fantasy,
                Authors = new[] { Author.AUTHORS[0] },
                Publication = new DateTime(2000, 7, 15),
                Pages = 210,
            }
        };
    }
}
