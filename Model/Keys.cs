using System.Text.Json.Serialization;

namespace Citation.Model;

public class Keys()
{
    [JsonPropertyName("rkey")]
    public string ReEncryptKey { get; set; }

    [JsonPropertyName("riv")]
    public string ReEncryptIv { get; set; }
}