using Citation.Properties;

namespace Citation.Model.Preserve;

/// <summary>
/// Represents a version identifier for software, including release month, test state, life cycle stage, and main
/// version number.
/// </summary>
/// <remarks>The SoftwareVersion record encapsulates key metadata about a software release, such as its
/// development stage and release timing. It provides value-based equality and comparison operators based on the main
/// version number, allowing instances to be compared for ordering. This type is immutable; all properties are set at
/// initialization and cannot be modified.</remarks>
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
            _ => throw new NotImplementedException(Resources.SoftwareVersionData_NotImplementation)
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
