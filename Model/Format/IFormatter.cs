namespace Citation.Model.Format
{
    public interface IFormatter
    {
        public string ToMarkdown();
        public string ToLatex();
    }
}
