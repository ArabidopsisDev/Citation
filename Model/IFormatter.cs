namespace Citation.Model
{
    public interface IFormatter
    {
        public string FormatName { get; }
        public string ToMarkdown();
        public string ToLatex();
    }
}
