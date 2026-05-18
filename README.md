# Riftstrapper
Run SteamVR or a PowerShell script instead of Oculus Dash. This is a short executable to replicate the functionality of [OculusKiller](https://github.com/BnuuySolutions/OculusKiller). You can even follow the same install instructions!

I know it's buggy, I just don't care.

## Install
(copied 1:1 from https://github.com/BnuuySolutions/OculusKiller#installation, just changed the path)

- Open Task Manager, go to Services and look for OVRService, right click on it and stop it. (If you have the Oculus app or any VR games open, they WILL close when stopping OVRService.)
- Go to C:\Program Files\Meta Horizon\Support\oculus-dash\dash\bin in Explorer.
- Rename the original OculusDash.exe to OculusDash.exe.bak and move my replacement OculusDash.exe into the folder you just opened in Explorer.
- Go back to Task Manager, look for OVRService again, right click on it and start it.

Enjoy your completely yeeted Oculus Dash with SteamVR auto-start, and the extra performance!

## Parameters
If you manually invoke the program, you have a couple extra parameters. The default (paramless) is to start SteamVR.

```
riftstrapper.exe [--steamvr] [--nohook] [startup script path relative to the program]
```

- `--steamvr`: Launch SteamVR. If a startup script is specified, this must be added to start SteamVR.
- `--nohook`: Don't close the Oculus Link connection when the SteamVR server exits

Riftstrapper will also check the user's documents folder for a startup script. `%USERPROFILE%\Documents\Riftstrapper.ps1` will automatically be ran if it exists (unless a custom startup script is specified)