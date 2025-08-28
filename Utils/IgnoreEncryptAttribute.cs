namespace Citation.Utils
{
    [System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    sealed class IgnoreEncryptAttribute : Attribute
    {
        readonly string positionalString;

        public IgnoreEncryptAttribute(string why)
        {
            this.positionalString = why;
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        public int NamedInt { get; set; }
    }
}
