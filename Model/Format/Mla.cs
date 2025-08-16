using Citation.Model.Reference;
using System.Linq;
using System.Text;

namespace Citation.Model.Format
{
    public class Mla : IFormatter
    {
        public Mla(JournalArticle article)
        {
            _article = article;
            Authors = article.Message?.Author?.Select(author => author.ToString()).ToArray();
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
        public string FormatName { get; } = "MLA 8th Edition";

        public string ToMarkdown()
        {
            var authorString = FormatAuthors();
            var titleString = $"\"{PaperName}.\"";
            var journalString = $"*{JournalName}*,";
            var volumeIssueString = FormatVolumeIssue();
            var yearString = $"{Year},";
            var pageString = FormatPage();
            var urlString = FormatUrl();

            var parts = new[] { authorString, titleString, journalString,
                             volumeIssueString, yearString, pageString, urlString }
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();

            return CleanTrailingPunctuation(string.Join(" ", parts));
        }

        public string ToLatex()
        {
            var authorString = FormatAuthors();
            var titleString = $"``{PaperName}.''";
            var journalString = $@"\textit{{{JournalName}}},";
            var volumeIssueString = FormatVolumeIssue();
            var yearString = $"{Year},";
            var pageString = FormatPage();
            var urlString = FormatUrl();

            var parts = new[] { authorString, titleString, journalString,
                             volumeIssueString, yearString, pageString, urlString }
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();

            return CleanTrailingPunctuation(string.Join(" ", parts));
        }

        private string FormatAuthors()
        {
            if (Authors == null || Authors.Length == 0)
                return string.Empty;

            return Authors.Length switch
            {
                1 => $"{Authors[0]}.",
                2 => $"{Authors[0]}, and {Authors[1]}.",
                _ => $"{Authors[0]}, et al."
            };
        }

        private string FormatVolumeIssue()
        {
            var parts = new StringBuilder();

            if (!string.IsNullOrEmpty(Volume))
                parts.Append($"vol. {Volume}");

            if (!string.IsNullOrEmpty(Issue))
            {
                if (parts.Length > 0) parts.Append(", ");
                parts.Append($"no. {Issue}");
            }

            return parts.Length > 0 ? $"{parts}," : string.Empty;
        }

        private string FormatPage()
        {
            if (string.IsNullOrEmpty(Page))
                return string.Empty;

            var prefix = Page.Contains('-') ? "pp. " : "p. ";
            return $"{prefix}{Page},";
        }

        private string FormatUrl()
        {
            return !string.IsNullOrEmpty(Url)
                ? Url.Replace("https://", "").Replace("http://", "")
                : string.Empty;
        }

        private string CleanTrailingPunctuation(string text)
        {
            text = text.Trim();

            if (text.EndsWith(","))
                text = text[..^1] + ".";
            else if (!text.EndsWith("."))
                text += ".";

            return text;
        }
    }
}