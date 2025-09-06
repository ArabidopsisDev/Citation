using Citation.Utils;
using System.Data.OleDb;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows;
using System.Xml.Linq;

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

            var mainWindow = Application.Current.MainWindow as MainWindow;
            var password = mainWindow!.verify;

            var encrypt = Cryptography.EncryptObject(password, this.Message);
            containers = Cryptography.EncryptData(password, containers!);
            titles = Cryptography.EncryptData(password, titles!);
            authors = Cryptography.EncryptData(password, authors!);
            link = Cryptography.EncryptData(password, link);
            publicationTime = Cryptography.EncryptData(password, publicationTime);

            var sqlString = $"""
                             INSERT INTO tb_Paper(PaperIssue, PaperContainer, PaperAbstract, 
                             PaperDoi, PaperPage, PaperTitle, PaperVolume, PaperAuthor, 
                             PaperLink, PaperUrl, PaperPublished, PaperFolder)
                             VALUES ('{encrypt.Issue}', '{containers}', '{encrypt.Abstract}', 
                             '{encrypt.Doi}', '{encrypt.Page}', '{titles}', '{encrypt.Volume}',
                             '{authors}', '{link}', '{encrypt.Url}', '{publicationTime}', 
                             '{encrypt.Folder}')
                             """;
            return sqlString;
        }

        public void DeleteSql(OleDbConnection connection)
        {
            var sqlCommand = $"""
                              DELETE FROM tb_Paper
                              WHERE PaperUrl = ?
                              """;

            var command = new OleDbCommand(sqlCommand, connection);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var password = mainWindow!.verify;

            if (Message is null) return;
            command.Parameters.AddWithValue("?",
                Cryptography.EncryptData(password!, Message.Url!));
            command.ExecuteNonQuery();
        }

        public static JournalArticle FromArticle(JournalArticleDb db)
        {
            var author = db.Author.ToArray();
            Link[] link = [new Link() { Url = db.Link }];
            var published = new Published();

            try
            {
                published = new Published()
                {
                    DateParts = [[.. db.Published.Split('/')
                    .Select(int.Parse)
                    .ToArray()
                    ]]
                };
            }
            catch
            {
                // ignored
            }

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
            Abstract ??= $"Unable to get abstract {Url}.";

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
            return $"{Family} {Given}";
        }

        public static Author ConvertBack(string name)
        {
            if (name == string.Empty) return null;

            // Reconstructed the previously unscalable stupid logic
            var nameArray = name.Split(' ');

            try
            {
                var author = new Author()
                {
                    Family = nameArray[0],
                    Given = nameArray[1]
                };

                return author;
            }
            catch
            {
                return new Author()
                {
                    Family = Randomization.RandomBuddha(),
                    Given = Randomization.RandomBuddha()
                };
            }
        }
    }

    public class Link
    {
        [JsonPropertyName("URL")]
        public string? Url { get; set; }

        [JsonPropertyName("content-type")]
        public string? ContentType { get; set; }
    }

    /// <summary>
    /// Article database structure
    /// </summary>
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
        public List<Author> Author { get; set; }
        public string AuthorString { get; set; }
        public string Link { get; set; }
        public string Url { get; set; }
        public string Published { get; set; }

        /// <summary>
        /// (for sub-topics) Indicates which subtopic the article belongs to
        /// </summary>
        public string Folder { get; set; }

        public void Afterward()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            var decrypt = Cryptography.DecryptObject(mainWindow!.verify, this);

            // Reflection is a good way to copy properties
            foreach (var prop in typeof(JournalArticleDb).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    var value = prop.GetValue(decrypt, null);
                    prop.SetValue(this, value, null);
                }
            }

            Container = [.. ContainerString.Split('/').Where(x => x != "")];
            Title = [.. TitleString.Split('/').Where(x => x != "")];

            Author = [];
            foreach (var item in AuthorString.Split('/'))
            {
                if (item != string.Empty)
                    Author.Add(Model.Reference.Author.ConvertBack(item));
            }
        }
    }
}
