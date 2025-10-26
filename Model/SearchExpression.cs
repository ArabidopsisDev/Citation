using System.Text;
using System.Text.Json.Serialization;

namespace Citation.Model
{
    [Serializable]
    public class SearchExpression
    {
        [JsonPropertyName("terms")]
        public string? Terms { get; set; }

        [JsonPropertyName("journal-or-booktitle")]
        public string? Journal { get; set; }

        [JsonPropertyName("years")]
        public string? Years { get; set; }

        [JsonPropertyName("authors")]
        public string? Authors { get; set; }

        [JsonPropertyName("affiliations")]
        public string? Affiliations { get; set; }

        [JsonPropertyName("volumes")]
        public string? Volumes { get; set; }

        [JsonPropertyName("pages")]
        public string? Pages { get; set; }

        [JsonPropertyName("issues")]
        public string? Issues { get; set; }

        [JsonPropertyName("title-abstract-or-author-specified-keywords")]
        public string? KeyWords { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("references")]
        public string? References { get; set; }

        [JsonPropertyName("issn-or-isbn")]
        public string? ISSNorISBN { get; set; }

        public string ToScienceDirect()
        {
            var exprBuilder = new StringBuilder();
            exprBuilder.Append("https://www.sciencedirect.com/search?");

            if (Terms is not null)
                exprBuilder.Append($"qs={Terms}");
            if (Journal is not null)
                exprBuilder.Append($"&pub={Journal}");
            if (Authors is not null)
                exprBuilder.Append($"&authors={Authors}");
            if (Affiliations is not null)
                exprBuilder.Append($"&affiliations={Affiliations}");
            if (KeyWords is not null)
                exprBuilder.Append($"&tak={KeyWords}");
            if (Title is not null)
                exprBuilder.Append($"&title={Title}");
            if (References is not null)
                exprBuilder.Append($"&reference={References}");
            if (Years is not null)
                exprBuilder.Append($"&date={Years}");
            if (Volumes is not null)
                exprBuilder.Append($"&volume={Volumes}");
            if (Issues is not null)
                exprBuilder.Append($"&issue={Issues}");
            if (Pages is not null)
                exprBuilder.Append($"&page={Pages}");

            return exprBuilder.ToString().Replace("?&", "?");
        }
    }
}