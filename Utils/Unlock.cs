using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Citation.Utils;

/// <summary>
/// Shenren TV
/// </summary>
public class Unlock
{
    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(uint dwSessionHandle);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(uint dwSessionHandle, uint nFiles, string[] rgsFilenames,
        uint nApplications, [In] RmUniqueProcess[] rgApplications, uint nServices, string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmGetList(uint dwSessionHandle, out uint pnProcInfoNeeded,
        out uint pnProcInfo, [In, Out] RmProcessInfo[] rgAffectedApps, ref uint lpdwRebootReasons);

    [StructLayout(LayoutKind.Sequential)]
    private struct RmUniqueProcess
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RmProcessInfo
    {
        public RmUniqueProcess Process;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string strAppName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string strServiceShortName;
        public RmAppType ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }

    private enum RmAppType
    {
        RmUnknownApp = 0,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    private const int RmRebootReasonNone = 0;
    private const int ErrorMoreData = 234;
    private const int ErrorSuccess = 0;

    public static bool ReleaseFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("文件不存在", filePath);

        var sessionKey = Guid.NewGuid().ToString();
        var result = RmStartSession(out var sessionHandle, 0, sessionKey);

        if (result != ErrorSuccess)
            throw new Win32Exception(result, "无法启动重启管理器会话");

        try
        {
            string[] resources = [Path.GetFullPath(filePath)];
            result = RmRegisterResources(sessionHandle, (uint)resources.Length, resources, 0, null, 0, null);

            if (result != ErrorSuccess)
                throw new Win32Exception(result, "无法注册资源");

            uint lpdwRebootReasons = RmRebootReasonNone;

            result = RmGetList(sessionHandle, out var pnProcInfoNeeded, out _, null, ref lpdwRebootReasons);

            if (result == ErrorMoreData)
            {
                var processInfo = new RmProcessInfo[pnProcInfoNeeded];
                result = RmGetList(sessionHandle, out pnProcInfoNeeded, out _, processInfo, ref lpdwRebootReasons);

                if (result == ErrorSuccess)
                {
                    var success = true;
                    foreach (var info in processInfo)
                    {
                        try
                        {
                            var process = Process.GetProcessById(info.Process.dwProcessId);
                            Console.WriteLine($"正在终止进程: {process.ProcessName} (PID: {process.Id})");
                            process.Kill();
                            process.WaitForExit(5000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"无法终止进程 {info.Process.dwProcessId}: {ex.Message}");
                            success = false;
                        }
                    }
                    return success;
                }
            }
            return result == ErrorSuccess;
        }
        finally
        {
            RmEndSession(sessionHandle);
        }
    }
}