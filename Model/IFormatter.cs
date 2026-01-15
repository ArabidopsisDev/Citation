namespace Citation.Model;

/// <summary>
/// Defines a contract for formatting content into different output representations, such as Markdown or LaTeX.
/// </summary>
/// <remarks>Implementations of this interface provide methods to convert content into specific markup formats.
/// This interface is intended for use in scenarios where content needs to be exported or displayed in multiple document
/// formats.</remarks>
public interface IFormatter
{
    public string FormatName { get; }
    public string ToMarkdown();
    public string ToLatex();
}
