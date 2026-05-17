using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;

// Initialization
var launchSteamVr = true;
var scriptPath = "";

if (args.Length > 0)
{
    launchSteamVr = args.Contains("--steamvr");
    scriptPath = args.FirstOrDefault(a => a != "--openvr") ?? "";
}

if (!launchSteamVr && scriptPath == "")
    return; // nothing to do

// Choose to run script
if (scriptPath != "")
{
    var startInfo = new ProcessStartInfo
    {
        FileName = "powershell.exe",
        Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\"",
    };

    Process.Start(startInfo)?.WaitForExit();
}

// Choose to launch SteamVR
if (launchSteamVr)
{
    var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");

    if (!File.Exists(path))
        return; // nothing to do

    var jsonText = File.ReadAllText(path);

    var json = JsonNode.Parse(jsonText);
    if (json == null)
        return;

    var steamVrPath = (string)json["runtime"]?[0];

    if (steamVrPath == null)
        return;

    var startupPath = Path.Combine(steamVrPath, @"bin\win64\vrstartup.exe");
    var startupProcess = Process.Start(startupPath);

    if (startupProcess == null)
        return;

    var stopwatch = Stopwatch.StartNew();
    var vrMonitorProcesses = Process.GetProcessesByName("vrmonitor");
    
    while (vrMonitorProcesses.Length == 0 && !startupProcess.HasExited && stopwatch.Elapsed.Seconds < 10)
    {
        vrMonitorProcesses = Process.GetProcessesByName("vrmonitor");
        Thread.Sleep(1000);
    }

    if (vrMonitorProcesses.Length == 0)
        return;
    
    // Close Steam when Oculus exits
    foreach (var oculusProcess in GetOculusProcesses())
    {
        oculusProcess.EnableRaisingEvents = true;
        oculusProcess.Exited += (_, _) =>
        {
            foreach (var vrMonitorProcess in vrMonitorProcesses)
            {
                vrMonitorProcess.Kill();
                vrMonitorProcess.WaitForExit();
            }
        };
    }
    
    // Close Oculus when Steam exits
    foreach (var vrMonitorProcess in vrMonitorProcesses)
    {
        vrMonitorProcess.EnableRaisingEvents = true;
        vrMonitorProcess.Exited += (_, _) =>
        {
            var oculusProcesses = GetOculusProcesses();
            if (oculusProcesses == null)
                return;
            
            foreach (var oculusProcess in oculusProcesses)
            {
                oculusProcess.Kill();
                oculusProcess.WaitForExit();
            }

            var oculusServerProcesses = Process.GetProcessesByName("OVRServer_x64");
            foreach (var oculusServerProcess in oculusServerProcesses)
            {
                oculusServerProcess.Kill();
                oculusServerProcess.WaitForExit();
            }
        };
    }
}

return;

// Util functions
List<Process> GetOculusProcesses()
{
    var processes = Process.GetProcessesByName("Client.exe").Where(p => p.MainModule?.FileName == @"C:\Program Files\Meta Horizon\Support\oculus-client");
    return processes.ToList();
}