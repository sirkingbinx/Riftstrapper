using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;

// Initialization
var launchSteamVr = true;
var scriptPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Riftstrapper.ps1");
var noHook = false;

if (args.Length > 0)
{
    launchSteamVr = args.Contains("--steamvr");
    noHook = args.Contains("--nohook");
    scriptPath = args.FirstOrDefault(a => a != "--steamvr" && a != "--nohook") ?? "";
}

// Custom script execution
if (File.Exists(scriptPath))
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
    var vrStartupProcess = Process.Start(startupPath);

    if (noHook)
        return;

    if (vrStartupProcess == null)
        return;

    vrStartupProcess.WaitForExit();

    // Wait for SteamVR to quit
    while (IsSteamVRRunning()) {
        Thread.Sleep(1000);
    }

    // Restart the Oculus Runtime service (which does OVRServer handling and other stuff)
    // More proper way to kill a Link connection on exit without causing problems for the user
    var closeLinkProcess = Process.Start(new ProcessStartInfo
    {
        FileName = "cmd.exe",
        Arguments = "/c net stop \"Oculus VR Runtime Service\"",
        CreateNoWindow = true,
        UseShellExecute = false
    });
    closeLinkProcess.WaitForExit();

    var startLinkProcess = Process.Start(new ProcessStartInfo
    {
        FileName = "cmd.exe",
        Arguments = "/c net start \"Oculus VR Runtime Service\"",
        CreateNoWindow = true,
        UseShellExecute = false
    });
    startLinkProcess.WaitForExit();
}

return;

bool IsSteamVRRunning() {
    return Process.GetProcessesByName("vrserver").Any();
}
