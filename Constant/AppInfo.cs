using Citation.Model.Preserve;

namespace Citation.Constant
{
    public class AppInfo
    {
        public static SoftwareVersion AppVersion = new(
            SoftwareVersion.Month.September, 
            SoftwareVersion.State.Alpha, 
            SoftwareVersion.LifeCycle.BasicDevelop, 
            1097
        );
    }
}
