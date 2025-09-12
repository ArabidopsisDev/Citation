namespace Citation.Model.Preserve
{
    public record SoftwareVersion
    {
        public SoftwareVersion(Month month, State state, LifeCycle cycle, int version)
        {
            MonthInfo = month;
            TestState = state;
            SoftwareLifeCycle = cycle;
            MainVersion = version;
        }

        public SoftwareVersion(string versionString)
        {
            var version = versionString;
            if (version.Contains('_')) version = version.Split('_')[1];

            // ignore failure
            int.TryParse(version.AsSpan(3, 4), out int MainVersion);
        }

        public enum Month
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        public enum State
        {
            Alpha,
            Beta,
            Stable
        }

        public enum LifeCycle
        {
            BasicDevelop,
            Iteration,
            Maintain
        }

        private string LifeCycleConvert(LifeCycle lifeCycle)
        {
            return lifeCycle switch
            {
                LifeCycle.BasicDevelop => "tf",
                LifeCycle.Iteration => "it",
                LifeCycle.Maintain => "mt",
                _ => throw new NotImplementedException("该版本类型未被实现")
            };
        }

        public Month MonthInfo { get; init; }
        public State TestState { get; init; }
        public LifeCycle SoftwareLifeCycle { get; init; }
        public int MainVersion { get; init; }

        public override string ToString()
        {
            var lowerLifeCycle = LifeCycleConvert(SoftwareLifeCycle);
            var lowerState = TestState.ToString().ToLower()[0];
            var lowerMonth = MonthInfo.ToString().ToLower()[0];
            return $"{lowerLifeCycle}{lowerMonth}{MainVersion}{lowerState}";
        }

        public override int GetHashCode()
        {
            return MainVersion;
        }

        public static bool operator <(SoftwareVersion left, SoftwareVersion right)
        {
            return left.MainVersion < right.MainVersion;
        }

        public static bool operator >(SoftwareVersion left, SoftwareVersion right)
        {
            return left.MainVersion > right.MainVersion;
        }
    }
}
