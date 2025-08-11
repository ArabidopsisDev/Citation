using System.Text;
using System.Windows.Controls;
using Citation.Model.Reference;

namespace Citation.Model.Format
{
    public class Apa
    {


        public Apa(JournalArticle article)
        {
            _article = article;

            Authors = article.Message?.Author?
                 .Select(author => author.ToString())
                 .ToArray();
            Year = article.Message?.Published?.DateParts?[0][0].ToString();
            PaperName = article.Message?.Title?[0];
            JournalName = article.Message?.Container?[0];
            Volume = article.Message?.Volume;
            Issue = article.Message?.Issue;
            Page = article.Message?.Page;
            Url = article.Message?.Url;
        }

        private JournalArticle _article;
        public string[]? Authors { get; private set; }
        public string? Year { get; private set; }
        public string? PaperName { get; private set; }
        public string? JournalName { get; private set; }
        public string? Volume { get; private set; }
        public string? Issue { get; private set; }
        public string? Page { get; private set; }
        public string? Url { get; private set; }

        public string ToMarkdown()
        {
            var authorString = "";

            if (Authors?.Length == 1)
            {
                authorString = Authors[0];
            }
            else if (Authors?.Length > 1)
            {
                for (var i = 0; i < Authors?.Length; i++)
                {
                    if (i < Authors.Length - 2)
                        authorString += $"{Authors[i]}, ";
                    else if (i == Authors.Length - 2)
                        authorString += $"{Authors[i]}, & ";
                    else
                        authorString += Authors[i];
                }
            }

            var yearString = Year is not null ? $"({Year}d)." : "";
            var titleString = PaperName is not null ? $"{PaperName}." : "";
            var journalString = $"*{JournalName}*,";
            var volumeAndIssueString = $"*{Volume}*({Issue}),";
            var pageString = Page is not null ? $"{Page}." : "";

            var markdownApaBuilder = new StringBuilder();
            markdownApaBuilder.AppendJoin(" ", [authorString,
                yearString,titleString, journalString,
                volumeAndIssueString, pageString, Url]);

            return markdownApaBuilder.ToString();
        }
    }
}
