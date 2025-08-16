using Citation.Model.Reference;
using System.Text;

namespace Citation.Model.Format
{
    public class Chicago : IFormatter
    {
        public Chicago(JournalArticle article)
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

        public Chicago() { }

        private JournalArticle? _article = null;
        public List<string> Authors { get; private set; } = [];
        public string? Year { get; private set; }
        public string? PaperName { get; private set; }
        public string? JournalName { get; private set; }
        public string? Volume { get; private set; }
        public string? Issue { get; private set; }
        public string? Page { get; private set; }
        public string? Url { get; private set; }

        public string FormatName { get; } = "Chicago (Notes and Bibliography)";

        public string ToMarkdown()
        {
            string authorString;
            switch (Authors.Count)
            {
                case 0:
                    authorString = string.Empty;
                    break;
                case 1:
                    authorString = Authors[0];
                    break;
                default:
                    authorString = string.Join(", ", Authors.Take(Authors.Count - 1));
                    authorString += $", and {Authors.Last()}";
                    break;
            }

            string titleString = string.IsNullOrEmpty(PaperName)
                ? string.Empty
                : $"\"{PaperName},\"";

            string journalString = string.IsNullOrEmpty(JournalName)
                ? string.Empty
                : $"*{JournalName}*";

            string volumeIssueString = "";
            if (!string.IsNullOrEmpty(Volume) && !string.IsNullOrEmpty(Issue))
            {
                volumeIssueString = $" *{Volume}*, no. {Issue}";
            }
            else if (!string.IsNullOrEmpty(Volume))
            {
                volumeIssueString = $" *{Volume}*";
            }
            else if (!string.IsNullOrEmpty(Issue))
            {
                volumeIssueString = $" no. {Issue}";
            }

            string yearPageString = "";
            if (!string.IsNullOrEmpty(Year))
            {
                yearPageString += $"({Year})";
                if (!string.IsNullOrEmpty(Page))
                {
                    yearPageString += $": {Page}";
                }
            }

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(authorString)) parts.Add(authorString);
            if (!string.IsNullOrEmpty(titleString)) parts.Add(titleString);
            if (!string.IsNullOrEmpty(journalString)) parts.Add(journalString + volumeIssueString);
            if (!string.IsNullOrEmpty(yearPageString)) parts.Add(yearPageString);
            if (!string.IsNullOrEmpty(Url)) parts.Add(Url);

            return FormatWithPunctuation(parts);
        }

        public string ToLatex()
        {
            string authorString;
            switch (Authors.Count)
            {
                case 0:
                    authorString = string.Empty;
                    break;
                case 1:
                    authorString = Authors[0];
                    break;
                default:
                    authorString = string.Join(", ", Authors.Take(Authors.Count - 1));
                    authorString += $", and {Authors.Last()}";
                    break;
            }

            string titleString = string.IsNullOrEmpty(PaperName)
                ? string.Empty
                : $"\"{PaperName},\"";

            string journalString = string.IsNullOrEmpty(JournalName)
                ? string.Empty
                : @"\textit{" + JournalName + "}";

            string volumeIssueString = "";
            if (!string.IsNullOrEmpty(Volume) && !string.IsNullOrEmpty(Issue))
            {
                volumeIssueString = $@" \textit{{{Volume}}}, no. {Issue}";
            }
            else if (!string.IsNullOrEmpty(Volume))
            {
                volumeIssueString = $@" \textit{{{Volume}}}";
            }
            else if (!string.IsNullOrEmpty(Issue))
            {
                volumeIssueString = $" no. {Issue}";
            }

            string yearPageString = "";
            if (!string.IsNullOrEmpty(Year))
            {
                yearPageString += $"({Year})";
                if (!string.IsNullOrEmpty(Page))
                {
                    yearPageString += $": {Page}";
                }
            }

            var parts = new List<string>();
            if (!string.IsNullOrEmpty(authorString)) parts.Add(authorString);
            if (!string.IsNullOrEmpty(titleString)) parts.Add(titleString);
            if (!string.IsNullOrEmpty(journalString)) parts.Add(journalString + volumeIssueString);
            if (!string.IsNullOrEmpty(yearPageString)) parts.Add(yearPageString);
            if (!string.IsNullOrEmpty(Url)) parts.Add(Url);

            return FormatWithPunctuation(parts);
        }

        private static List<string> BuildAuthorString(Author[] authors)
        {
            var authorList = new List<string>();
            if (authors == null || authors.Length == 0)
            {
                authorList.Add("Anonymous");
                return authorList;
            }

            for (int i = 0; i < authors.Length; i++)
            {
                var author = authors[i];

                if (string.IsNullOrWhiteSpace(author.Family))
                {
                    authorList.Add(string.IsNullOrWhiteSpace(author.Given)
                        ? "Anonymous"
                        : author.Given);
                    continue;
                }

                if (i == 0)
                {
                    authorList.Add(string.IsNullOrWhiteSpace(author.Given)
                        ? author.Family
                        : $"{author.Family}, {author.Given}");
                }
                else
                {
                    authorList.Add(string.IsNullOrWhiteSpace(author.Given)
                        ? author.Family
                        : $"{author.Given} {author.Family}");
                }
            }

            return authorList;
        }

        private string FormatWithPunctuation(List<string> parts)
        {
            if (parts.Count == 0) return string.Empty;

            var builder = new StringBuilder(parts[0]);

            for (int i = 1; i < parts.Count; i++)
            {
                if (i == parts.Count - 1 && !string.IsNullOrEmpty(Url))
                {
                    builder.Append(" " + parts[i]);
                }
                else
                {
                    builder.Append(", " + parts[i]);
                }
            }

            if (!builder.ToString().EndsWith('.') && !builder.ToString().EndsWith(')'))
            {
                builder.Append('.');
            }

            return builder.ToString();
        }
    }
}