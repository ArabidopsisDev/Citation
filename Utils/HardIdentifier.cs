using System.Management;

namespace Citation.Utils;

internal class HardIdentifier
{
    private string GetCpuSerialNumber()
    {
        try
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_Processor");
            var sCpuSerialNumber = "";
            foreach (var manage in searcher.Get())
            {
                var mo = (ManagementObject)manage;
                sCpuSerialNumber = mo["ProcessorId"].ToString()!.Trim();
                break;
            }
            return sCpuSerialNumber;
        }
        catch
        {
            return "";
        }
    }
    private string GetBiosSerialNumber()
    {
        try
        {
            var searcher = new ManagementObjectSearcher("Select * From Win32_BIOS");
            string sBiosSerialNumber = "";
            foreach (var manage in searcher.Get())
            {
                var mo = (ManagementObject)manage;
                sBiosSerialNumber = mo.GetPropertyValue("SerialNumber").ToString()!.Trim();
                break;
            }
            return sBiosSerialNumber;
        }
        catch
        {
            return "";
        }
    }
    private string GetHardDiskSerialNumber()
    {
        try
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            var sHardDiskSerialNumber = "";
            foreach (var manage in searcher.Get())
            {
                var mo = (ManagementObject)manage;
                sHardDiskSerialNumber = mo["SerialNumber"].ToString()!.Trim();
                break;
            }
            return sHardDiskSerialNumber;
        }
        catch
        {
            return "";
        }
    }

    public override string ToString()
    {
        var hard = GetHardDiskSerialNumber().Replace("_", "").Replace(".", "");
        var line1 = GetCpuSerialNumber().Length > 6 ?
            GetCpuSerialNumber()[^6..] : GetCpuSerialNumber();
        var line2 = GetBiosSerialNumber().Length > 6 ?
            GetBiosSerialNumber()[^6..] : GetBiosSerialNumber();
        var line3 = hard.Length > 6 ? hard[^6..] : hard;
        return $"{line1}-{line2}-{line3}";
    }
}
