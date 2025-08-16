using System.Text.Json.Serialization;
using System.Windows.Controls;

namespace Citation.Model.Reference
{
    public class JournalArticle
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        public string ToSql()
        {
            var containers = Message.Container?
                .Aggregate("", (current, str) => current + ("/" + str));
            var titles = Message.Title?
                .Aggregate("", (title, str) => title + ("/" + str));
            var authors = Message.Author?
                .Aggregate("", (author, str) => author + ("/" + str));

            string link = "";
            foreach (var lik in Message.Link!)
            {
                if (lik.Url!.Contains("pdf"))
                {
                    link = lik.Url;
                    break;
                }
            }

            string publicationTime = "";
            foreach (var dt in Message.Published.DateParts[0])
                publicationTime += $"{dt}/";
            publicationTime = publicationTime.TrimEnd('/');

            // Oh my god, this code is of poor quality
            var sqlString = $"""
                             INSERT INTO tb_Paper(PaperIssue, PaperContainer, PaperAbstract, 
                             PaperDoi, PaperPage, PaperTitle, PaperVolume, PaperAuthor, 
                             PaperLink, PaperUrl, PaperPublished, PaperFolder)
                             VALUES ('{Message.Issue}', '{containers}', '{Message.Abstract}', 
                             '{Message.Doi}', '{Message.Page}', '{titles}', '{Message.Volume}',
                             '{authors}', '{link}', '{Message.Url}', '{publicationTime}', 
                             '{Message.Folder}')
                             """;
            return sqlString;
        }

        public static JournalArticle FromArticle(JournalArticleDb db)
        {
            var author = db.Author
                .Select(Author.ConvertBack)
                .ToArray();
            Link[] link = [new Link() { Url = db.Link }];

            var published = new Published()
            {
                DateParts = [[.. db.Published.Split('/')
                    .Select(int.Parse)
                    .ToArray()
                ]]
            };

            var message = new Message()
            {
                Abstract = db.Abstract,
                Author = author,
                Container = db.Container,
                Doi = db.Doi,
                Folder = db.Folder,
                Issue = db.Issue,
                Link = link,
                Page = db.Page,
                Published = published,
                Title = db.Title,
                Url = db.Url,
                Volume = db.Volume
            };

            var article = new JournalArticle()
            {
                Message = message,
                Status = "extract succeed"
            };

            return article;
        }
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
        
        public string Folder { get; set; } = "Default";

        public string AuthorString { get; set; } = "anonymous";

        public void AfterWards()
        {
            Abstract ??= "Unable to get abstract.";

            switch (Author?.Length)
            {
                case 1:
                    AuthorString = Author[0].ToString();
                    break;
                case > 1:
                {
                    AuthorString = string.Empty;
                    for (var i = 0; i < Author?.Length; i++)
                    {
                        if (i < Author.Length - 2)
                            AuthorString += $"{Author[i]}, ";
                        else if (i == Author.Length - 2)
                            AuthorString += $"{Author[i]}, & ";
                        else
                            AuthorString += Author[i];
                    }
                    break;
                }
            }
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

    public class JournalArticleDb
    {
        public string Issue { get; set; }
        public string ContainerString { get; set; }
        public string[] Container { get; set; }
        public string Abstract { get; set; }
        public string Doi { get; set; }
        public string Page { get; set; }
        public string TitleString { get; set; }
        public string[] Title { get; set; }
        public string Volume { get; set; }
        public string[] Author { get; set; }
        public string AuthorString { get; set; }
        public string Link { get; set; }
        public string Url { get; set; }
        public string Published { get; set; }
        public string Folder { get; set; }

        public void Afterward()
        {
            Container = [.. ContainerString.Split('/').Where(x => x != "")];
            Title = [.. TitleString.Split('/').Where(x => x != "")];
            Author = [.. AuthorString.Split('/').Where(x => x != "")];
        }
    }
}
