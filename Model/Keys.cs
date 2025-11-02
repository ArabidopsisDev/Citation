using System.Text.Json.Serialization;

namespace Citation.Model;

public class Keys()
{
    [JsonPropertyName("rkey")]
    public string ReEncryptKey { get; set; } = string.Empty;

    [JsonPropertyName("riv")]
    public string ReEncryptIv { get; set; } = string.Empty;
}