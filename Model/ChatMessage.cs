namespace Citation.Model;

/// <summary>
/// Represents a message exchanged in a chat, including its content and type.
/// </summary>
public class ChatMessage
{
    public string? Content { get; set; }
    public string? Type { get; set; }
}
