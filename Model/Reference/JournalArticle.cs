using System.Text.Json.Serialization;

namespace Citation.Model.Reference
{
    public class JournalArticle
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("issue")]
        public string? Issue { get; set; }

        [JsonPropertyName("short-container-title")]
        public string[]? Container { get; set; }

        [JsonPropertyName("abstract")]
        public string? Abstract { get; set; }

        [JsonPropertyName("DOI")]
        public string? Doi { get; set; }

        [JsonPropertyName("page")]
        public string? Page { get; set; }

        [JsonPropertyName("title")]
        public string[]? Title { get; set; }

        [JsonPropertyName("volume")] 
        public string? Volume { get; set; }

        [JsonPropertyName("author")]
        public Author[]? Author { get; set; }

        [JsonPropertyName("link")]
        public Link[]? Link { get; set; }

        [JsonPropertyName("URL")]
        public string? Url { get; set; }

        [JsonPropertyName("published")]
        public Published? Published { get; set; }

        public string? AuthorString { get; set; }

        public void AfterWards()
        {
             Author.ToList().ForEach(aut => AuthorString += aut.ToString() +'\n');
             AuthorString = AuthorString.TrimEnd();

             Abstract = Abstract.Split("<jats:p>")[1].Split("</jats:p>")[0];
             
             return;
        }
    }

    public class Published
    {
        [JsonPropertyName("date-parts")]
        public int[][]? DateParts { get; set; }
    }

    public class Author
    {
        [JsonPropertyName("given")]
        public string? Given { get; set; }

        [JsonPropertyName("family")]
        public string? Family { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Family))
                return string.IsNullOrWhiteSpace(Given) ? "Anonymous" : Given;

            if (string.IsNullOrWhiteSpace(Given))
                return Family;

            var initials = Given.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part[0].ToString().ToUpper() + ".")
                .ToArray();

            return $"{Family}, {string.Join(" ", initials)}";
        }

        public static Author ConvertBack(string name)
        {
            // Not even acting
            var author = new Author()
            {
                Family = name,
                Given = null
            };

            return author;
        }
    }

    public class Link
    {
        [JsonPropertyName("URL")]
        public string? Url { get; set; }

        [JsonPropertyName("content-type")]
        public string? ContentType { get; set; }
    }
}
