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

            switch (Authors?.Length)
            {
                case 1:
                    authorString = Authors[0];
                    break;
                case > 1:
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
                        break;
                    }
            }

            // If you don't have any of these things, then just go home, okay?
            var yearString = $"({Year}).";
            var titleString = $"{PaperName}.";
            var journalString = $"*{JournalName}*,";

            string? volumeAndIssueString;
            if (!string.IsNullOrEmpty(Volume) && string.IsNullOrEmpty(Issue))
                volumeAndIssueString = $"*{Volume}*,";
            else if (string.IsNullOrEmpty(Volume) && !string.IsNullOrEmpty(Issue))
                volumeAndIssueString = $"*{Issue}*,";
            else if (string.IsNullOrEmpty(Volume) && string.IsNullOrEmpty(Issue))
                volumeAndIssueString = null;
            else
                volumeAndIssueString = $"*{Volume}*({Issue}),";

            if (string.IsNullOrEmpty(Page))
            {
                if (volumeAndIssueString is not null)
                    volumeAndIssueString =
                        $"{volumeAndIssueString.Substring(0,
                            volumeAndIssueString.Length - 1)}.";
                else
                    journalString =
                        $"{journalString.Substring(0,
                            journalString.Length - 1)}.";
            }
            var pageString = !string.IsNullOrEmpty(Page) ? $"{Page}." : null;

            var markdownApaBuilder = new StringBuilder();
            markdownApaBuilder.AppendJoin(" ", [authorString, yearString, titleString,
                journalString, volumeAndIssueString, pageString, Url]);
            return markdownApaBuilder.ToString();
        }

        public string ToLatex()
        {
            var authorString = "";

            switch (Authors?.Length)
            {
                case 1:
                    authorString = Authors[0];
                    break;
                case > 1:
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
                        break;
                    }
            }

            // If you don't have any of these things, then just go home, okay?
            var yearString = $"({Year}).";
            var titleString = $"{PaperName}.";
            var journalString = @" \textit{" + JournalName+ "},";

            string? volumeAndIssueString;
            if (!string.IsNullOrEmpty(Volume) && string.IsNullOrEmpty(Issue))
                volumeAndIssueString = @" \textit{" + Volume+ "},";
            else if (string.IsNullOrEmpty(Volume) && !string.IsNullOrEmpty(Issue))
                volumeAndIssueString = @" \textit{" + Issue+ "},";
            else if (string.IsNullOrEmpty(Volume) && string.IsNullOrEmpty(Issue))
                volumeAndIssueString = null;
            else
                volumeAndIssueString = @" \textit{" + Volume+ "}" + $"({Issue}),";

            if (string.IsNullOrEmpty(Page))
            {
                if (volumeAndIssueString is not null)
                    volumeAndIssueString =
                        $"{volumeAndIssueString.Substring(0,
                            volumeAndIssueString.Length - 1)}.";
                else
                    journalString =
                        $"{journalString.Substring(0,
                            journalString.Length - 1)}.";
            }
            var pageString = !string.IsNullOrEmpty(Page) ? $"{Page}." : null;

            var latexApaBuilder = new StringBuilder();
            latexApaBuilder.AppendJoin(" ", [authorString, yearString, titleString,
                journalString, volumeAndIssueString, pageString, Url]);
            return latexApaBuilder.ToString();
        }
    }
}