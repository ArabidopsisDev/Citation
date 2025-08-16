using Citation.Model.Reference;
using System.Text;

namespace Citation.Model.Format
{
    /// <summary>
    /// MLA 8th Edition formatter
    /// </summary>
    public class Mla : IFormatter
    {
        public Mla(JournalArticle article)
        {
            _article = article;

            Authors = BuildAuthorString(article.Message?.Author!);
            Year = article.Message?.Published?.DateParts?[0][0].ToString();
            PaperName = article.Message?.Title?[0];
            JournalName = article.Message?.Container?[0];
            Volume = article.Message?.Volume;
            Issue = article.Message?.Issue;
            Page = article.Message?.Page;
            Url = article.Message?.Url;
        }

        public Mla() { }

        private JournalArticle _article;
        public string[]? Authors { get; private set; }
        public string? Year { get; private set; }
        public string? PaperName { get; private set; }
        public string? JournalName { get; private set; }
        public string? Volume { get; private set; }
        public string? Issue { get; private set; }
        public string? Page { get; private set; }
        public string? Url { get; private set; }

        public string FormatName { get; } = "MLA 8th Edition";

        public string ToMarkdown()
        {
            return BuildCitationString(FormatItalicMarkdown);
        }

        public string ToLatex()
        {
            return BuildCitationString(FormatItalicLatex);
        }

        private string BuildCitationString(Func<string, string> formatItalic)
        {
            var authorString = JoinAuthorsMlaStyle(Authors);
            var titleString = !string.IsNullOrEmpty(PaperName) ? $"\"{PaperName}.\"" : "\"[No title].\"";
            var journalString = !string.IsNullOrEmpty(JournalName) ? formatItalic(JournalName) : formatItalic("[No journal]");
            var yearString = !string.IsNullOrEmpty(Year) ? Year : "n.d.";

            // Build volume/issue info
            var volumeIssue = new StringBuilder();
            if (!string.IsNullOrEmpty(Volume))
            {
                volumeIssue.Append($"vol. {Volume}, ");
            }
            if (!string.IsNullOrEmpty(Issue))
            {
                volumeIssue.Append($"no. {Issue}, ");
            }

            // Build page info
            var pageString = "";
            if (!string.IsNullOrEmpty(Page))
            {
                pageString = Page.Contains('-') || Page.Contains(',')
                    ? $"pp. {Page}, "
                    : $"p. {Page}, ";
            }

            // Construct main citation
            var citation = new StringBuilder();
            citation.Append(authorString).Append(". ");
            citation.Append(titleString).Append(" ");
            citation.Append(journalString).Append(", ");
            citation.Append(volumeIssue);
            citation.Append(yearString);

            // Add page and URL if available
            if (!string.IsNullOrEmpty(pageString))
            {
                citation.Append(", ").Append(pageString);
            }
            if (!string.IsNullOrEmpty(Url))
            {
                citation.Append(Url);
            }

            // Ensure proper ending punctuation
            var result = citation.ToString().Trim();
            if (!result.EndsWith('.'))
            {
                result += ".";
            }

            return result;
        }

        private string FormatItalicMarkdown(string text)
        {
            return $"*{text}*";
        }

        private string FormatItalicLatex(string text)
        {
            return $@"\textit{{{text}}}";
        }

        private string JoinAuthorsMlaStyle(string[]? authors)
        {
            if (authors == null || authors.Length == 0)
                return "Anonymous";

            if (authors.Length == 1)
                return authors[0];

            if (authors.Length == 2)
                return $"{authors[0]} and {authors[1]}";

            var allButLast = string.Join(", ", authors.Take(authors.Length - 1));
            return $"{allButLast}, and {authors[^1]}";
        }

        private string[] BuildAuthorString(Author[] authors)
        {
            var authorList = new List<string>();

            foreach (var author in authors)
            {
                if (string.IsNullOrWhiteSpace(author.Family))
                {
                    authorList.Add(string.IsNullOrWhiteSpace(author.Given)
                        ? "Anonymous"
                        : author.Given);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(author.Given))
                {
                    authorList.Add(author.Family!);
                    continue;
                }

                var initials = author.Given!.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries)
                    .Where(part => !string.IsNullOrWhiteSpace(part))
                    .Select(part => part[0].ToString().ToUpper() + ".")
                    .ToArray();

                authorList.Add($"{author.Family}, {string.Join(" ", initials)}");
            }

            return authorList.ToArray();
        }
    }
}