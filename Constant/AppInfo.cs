using Citation.Model.Preserve;

namespace Citation.Constant
{
    public class AppInfo
    {
        public static SoftwareVersion AppVersion = new(
            SoftwareVersion.Month.September, 
            SoftwareVersion.State.Beta, 
            SoftwareVersion.LifeCycle.BasicDevelop, 
            1100
        );
    }
}
