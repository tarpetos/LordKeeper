# LordKeeper

Single-player module for Mount & Blade II: Bannerlord. It lets you control how often prisoners escape from you and your clan, so the lords you capture do not slip away within days.

## What it does

Each slider in the Mod Configuration Menu sets the share of the vanilla escape chance that still applies: 0% means they never escape, 100% means vanilla.

- **Captured lords** (the vanilla daily escape roll), separately for lords held:
  - in your party (default 50%),
  - in your clan's other parties (default 100%),
  - in your clan's towns and castles (default 50%),
  - by the other clans of your kingdom (default 100%).
- **Prisoners over the party limit** (the vanilla hourly escape roll while a party holds more prisoners than its limit), separately for your party (default 50%) and your clan's other parties (default 100%). In vanilla, this is the only way regular prisoners escape, and lords are only picked once no regular prisoners are left.
- **Caught escape messages**: When a lord held by you or your clan would have escaped but is stopped by the mod, a green message tells you the escape attempt was foiled. It can be turned off in the settings.
- **Vanilla math**: The vanilla escape rolls (party size, perks, governor bonuses) are kept and scaled by the sliders, not replaced.
- **Everything else stays vanilla**: Prisoners held outside your kingdom, your own captivity, ransom, peace, rescues and prison breaks are not changed.

## Release contents

The build scripts produce a self-contained module in the `dist/LordKeeper` directory.

## Compatibility and dependencies

- Single-player only.
- Requires **Harmony** (`Bannerlord.Harmony`) and the **Mod Configuration Menu (MCM)** (`Bannerlord.MBOptionScreen`), loaded before this mod.
- Safe to add to or remove from existing saves.

## Build from WSL/Linux

You must provide the path to your Bannerlord installation using the `BANNERLORD_DIR` environment variable. With Mono installed, run:

```bash
chmod +x build-linux.sh
BANNERLORD_DIR="/path/to/Mount & Blade II Bannerlord" ./build-linux.sh
```

**Important note for WSL users:** Windows restricts executing `.dll` files generated or copied from WSL environments (Mark of the Web issue). After copying the module to your game directory on Windows, you must unblock the files. Open PowerShell and run:

```powershell
Get-ChildItem -Path "C:\Your\Path\To\Mount & Blade II Bannerlord\Modules\LordKeeper" -Recurse | Unblock-File
```

## Build from Windows

Run the PowerShell script. If your game is not in the default Steam directory, specify the `-BannerlordDir` parameter:

```powershell
.\build-windows.ps1 -BannerlordDir "\path\to\Mount & Blade II Bannerlord"
```

## Install

Copy the contents of `dist/LordKeeper` to:

```text
Mount & Blade II Bannerlord/Modules/LordKeeper
```

Then enable **LordKeeper** in the Bannerlord launcher, after the official single-player modules, **Harmony** and **MCM**.
