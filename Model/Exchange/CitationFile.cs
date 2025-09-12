using Citation.Model.Reference;
using System.Text.Json.Serialization;

namespace Citation.Model.Exchange
{
    [Serializable]
    class CitationFile
    {
        [JsonPropertyName("version")]
        public string? Version { get; set; }

        [JsonPropertyName("articles")]
        public List<JournalArticle>? Articles { get; set; }
    }
}
