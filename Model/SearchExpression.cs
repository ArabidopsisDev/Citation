using System.Text;
using System.Text.Json.Serialization;

namespace Citation.Model;

/// <summary>
/// Represents a set of search criteria for querying scientific literature or publications.
/// </summary>
/// <remarks>The SearchExpression class encapsulates various fields commonly used to filter or refine literature
/// searches, such as terms, authors, journal or book title, publication years, and other bibliographic details.
/// Instances of this class can be used to construct search queries for external services, such as ScienceDirect, by
/// populating the relevant properties. All properties are optional; only those set will be included in the generated
/// search expression.</remarks>
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

        List<string> keywords = ["qs", "pub", "authors", "affiliations", "tak", "title",
            "reference", "date", "volume", "issue", "page"];
        List<string?> values = [Terms, Journal, Authors, Affiliations, KeyWords, Title,
            References, Years, Volumes, Issues, Pages];

        foreach (var (k, v) in keywords.Zip(values))
        {
            if (v is not null)
            {
                if (k == "qs")
                    exprBuilder.Append($"{k}={v}");
                else
                    exprBuilder.Append($"&{k}={v}");
            }
        }

        return exprBuilder.ToString().Replace("?&", "?");
    }
}