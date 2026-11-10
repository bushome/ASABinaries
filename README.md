# Updated runtime DLLs for ARK: Survival Ascended (Steam, Game Pass, Dedicated Server, DevKit)

Ark does tend to crash alot on some systems. Outside of trying to run it on suboptimal system configurations, part of that could be the older versions of files that are currently packaged with the game. Note: any files you copy into the game directly will likely get replaced during patches and will need re-applying afterward to maintain updated versions.

Everything in this repo has been checked against what the game actually loads (import tables of the client and server exes, plus live module lists from a running client, Game Pass client and dedicated server). Files the game ships but never loads are not included, because updating them does nothing. See [Shipped but never loaded](#shipped-but-never-loaded).

So at a glance what this Repo covers in terms of updates to the shipped binaries

| Component     | Shipped       | Repo           | Steam | Game Pass | Server |
|---------------|---------------|----------------|:-----:|:---------:|:------:|
| VS Runtime    | 14.50.35719   | 14.51.36247    | ✓     | n/a (1)   | ✓      |
| TBB malloc    | 2022.3.0.0    | 2023.2.0.0     | ✓     | ✓         | ✓      |
| EOS SDK       | 1.16.2.0      | 1.19.2.1       | ✓     | ✓         | ✓      |
| XAudio2 (EOS) | 1.0.0.1       | 1.0.2504.10003 | ✓     | ✓         | n/a (2)|
| DX12 Agility  | 618           | 620            | ✓     | ✗ (3)     | n/a (4)|

1. The Game Pass version gets the VC++ runtime from Windows, not from its own folder.
2. EOS only loads it for voice chat, which a dedicated server doesn't use.
3. The Game Pass exe is encrypted and locked by Windows, so it can't be patched. See the Agility SDK section.
4. The dedicated server exe doesn't use the Agility SDK at all.

## How to Use

The folder structures are setup just like the ones in the game or server directory so you can drop them right on top of the root folder:

- `Client\Steam\ARK Survival Ascended\` -> your Steam install folder
- `Client\XboxGames\ARK- Survival Ascended\` -> `C:\XboxGames\ARK- Survival Ascended\`
- `Server\` -> your dedicated server's root folder
- `Editor\` -> your ASA DevKit root folder (see [ASA DevKit](#asa-devkit-editor))

REMEMBER, DO NOT COPY OVER THE `D3D12` FOLDER IF YOU HAVE NO INTENTION OF PLAYING THE GAME ON A MODIFIED EXECUTABLE TO COVER THE NEW VERSION. YOU WILL BE UNABLE TO JOIN ANY SERVERS WITH BATTLE EYE ENABLED. Everything else is a plain file swap and works with BattlEye.

# Visual C++ Runtime
https://visualstudio.microsoft.com/downloads/

If you want to keep these up to date yourself on the latest stable release, download visual studio and search for them with the feature updates as they are released to VS. They will usually reside in C:\Program Files\Microsoft Visual Studio\18\Community\VC\Redist\MSVC\14.51.36231\x64\Microsoft.VC145.CRT while msdia140.dll will be in C:\Program Files\Microsoft Visual Studio\18\Community\DIA SDK\bin\amd64

Used by the game client and server (`ShooterGame\Binaries\Win64`):

- msvcp140.dll
- msvcp140_1.dll (used by the ONNX Runtime / NNE plugin)
- vcruntime140.dll
- vcruntime140_1.dll

ArkAPI (AsaApi) servers only, also included in `Server\`:

- msvcp140_atomic_wait.dll (loaded by plugins, e.g. the MariaDB connector)
- msdia140.dll (used by AsaApi when it reads the server's symbols to rebuild its offsets cache)

# Intel Threading Building Blocks
tbbmalloc.dll -> https://www.intel.com/content/www/us/en/developer/tools/oneapi/onetbb-download.html
part of oneAPI

The game and server only use `tbbmalloc.dll`. `tbb12.dll` ships with the game but is never loaded (confirmed on a running Steam client, Game Pass client and server); only the DevKit editor uses it.

- Steam: `ShooterGame\Binaries\Win64\tbbmalloc.dll`, plus `Engine\Binaries\Win64\tbbmalloc.dll` (used by the crash reporter)
- Game Pass: `Content\ShooterGame\Binaries\WinGDK\tbbmalloc.dll`
- Server: same locations as Steam

IMPORTANT -> The old tbb.dll that had been shipping with ark ascended since release no longer applies and MUST be removed post UE5.8 as of 11/6/2026.

# EPIC ONLINE SERVICES - Matchmaking, EOSID Account and Mod Authentication
https://onlineservices.epicgames.com/en-US/sdk C library.

For steam, the EOSSDK-Win64-Shipping.dll from the SDK\Bin folder goes in the \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64\RedpointEOS.

For Microsoft Gamepass users, place this under \XboxGames\ARK- Survival Ascended\Content\ShooterGame\Binaries\WinGDK (tested: the game loads 1.19.2.1 from there).

xaudio2_9redist.dll goes in `RedpointEOS\x64` on Steam and `WinGDK\x64` on Game Pass. EOS loads it for voice chat.
https://learn.microsoft.com/en-us/windows/win32/xaudio2/xaudio2-redistributable
If you have visual studio installed, use the nuget package manager in a new project to install the latest Xaudio 2 package.

# Using a newer Direct X 12 Agility SDK (patching the game exe)

**Steam client only.** The dedicated server doesn't use the Agility SDK, and the Game Pass exe can't be patched: Windows keeps it encrypted and locked, so Game Pass players stay on the 618 the game ships with. Don't copy a newer `D3D12` folder into the Game Pass install; the game won't start.

The game exe stores the Agility SDK version it expects. If the DLLs in its
`D3D12` folder are a different version, the game fails at launch with
"DirectX 12 is not supported on your system". To use newer DLLs you must
change that stored number to match. The game only uses the copy in
`ShooterGame\Binaries\Win64\D3D12\x64`; the one under `Engine\Binaries\Win64`
is never loaded, so it isn't in this repo. IF YOU ARE NOT GOING TO BE USING
THE NEWER AGILITY SDK FILES IN THE REPO WITH THIS THEN DO NOT DROP THEM INTO
YOUR GAME FOLDER!!!! CHOOSE ONLY THE FILES YOU ACTUALLY NEED!!!!!!!!!!

> **Warning**
> - This modifies `ArkAscended.exe`. **Do not use it with BattlEye.** Only
>   launch without BattlEye and only join servers that don't enforce it.
> - Steam's "Verify integrity of game files" and every game update will undo
>   the patch. Repeat these steps after each update.
> - Keep a copy of the original exe before you start.

## Easy way: AgilityPatch

`AgilityPatch.exe` (in the repo root, source in `AgilityPatch\`) does the
patching for you. It reads the D3D12 folder the exe itself points to,
takes the version from the `D3D12Core.dll` in that folder, and writes that
number into the exe. No hex editing and no version numbers to type.

1. Close the game (or the DevKit editor).
2. Copy `D3D12Core.dll` and `d3d12SDKLayers.dll` from this repo into
   `ShooterGame\Binaries\Win64\D3D12\x64` first.
3. Edit the path in `agilitypatch.bat` to match your install and run it,
   or run it yourself:

```
AgilityPatch "D:\SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64"
```

Point it at the `Win64` folder and it patches every exe there that uses
the Agility SDK, skipping the rest (including `ArkAscended_BE.exe`: BattlEye
won't run a patched game exe anyway). It saves the original as
`<exe>.bak`, verifies the write, and refuses to run while the exe is in use.

- `--check` only reports what would change.
- `--version N` uses N instead of reading `D3D12Core.dll`.
- Preview SDKs (1.7xx) are refused unless you add `--allow-preview`.

Run it again after every game update or file verify.

## Manual way: CFF Explorer

If you'd rather do it by hand, the steps below do the same thing.

### What you need

- [CFF Explorer](https://ntcore.com/explorer-suite/) (part of Explorer Suite)
- The `D3D12Core.dll` and `d3d12SDKLayers.dll` from this repo

### Step 1: Find your two version numbers

You need two numbers: the version your game **currently ships** and the
**new** version you are installing. In both cases the number is the
**middle part** of the SDK version (for example, 1.619.6 gives 619).

1. **Current version:** before replacing anything, right-click the original
   `D3D12Core.dll` in the game's `D3D12\x64` folder > Properties > Details,
   and read the product version.
2. **New version:** do the same for the `D3D12Core.dll` from this repo.

If both numbers are the same, you don't need to patch the exe. Skip to Step 4.

Use retail releases only (1.6xx). Preview releases (1.7xx) need Windows
Developer Mode and are not supported here.

### Step 2: Convert both numbers to hex bytes

The exe stores the number as 4 bytes in reverse (little-endian) order.
Convert **both** numbers from Step 1.

**Quick way (PowerShell):** replace `619` with your number.

```powershell
[BitConverter]::ToString([BitConverter]::GetBytes([uint32]619))
```

Output: `6B-02-00-00`. Those are the four bytes, in the order you'll see them.

**By hand (Windows Calculator):**

1. Open Calculator and switch to **Programmer** mode.
2. Select **DEC**, type the number (e.g. `619`), and read the **HEX** line (`26B`).
3. Pad it with zeros to 8 digits: `0000026B`.
4. Split into pairs: `00 00 02 6B`.
5. Reverse the order of the pairs: `6B 02 00 00`.

Examples:

| Number | Bytes in the exe |
|--------|------------------|
| 614    | `66 02 00 00`    |
| 618    | `6A 02 00 00`    |
| 619    | `6B 02 00 00`    |
| 620    | `6C 02 00 00`    |

### Step 3: Patch the exe

1. Open `ShooterGame\Binaries\Win64\ArkAscended.exe` in CFF Explorer.
2. Click **Export Directory** and find the row named `D3D12SDKVersion`.
3. Copy its **Function RVA** value.
4. Click **Address Converter**, paste the value into the **RVA** field, and
   press Enter. The hex view jumps to that location.
5. **Check the first 4 bytes.** They must match the bytes for your
   **current** version from Step 2. If they don't, stop: you are in the
   wrong place.
6. Click the first byte and type the bytes for your **new** version over
   the old ones.
7. **File > Save As** under a new name, then swap it in for the original
   (keep the original as a backup).

The location changes with every game build, so always find it through
Export Directory. Never reuse an offset from an earlier patch.

### Step 4: Install the DLLs

Copy `D3D12Core.dll` and `d3d12SDKLayers.dll` from this repo over the
originals in `ShooterGame\Binaries\Win64\D3D12\x64`.

### Troubleshooting

- **"DirectX 12 is not supported"**: the number in the exe and the DLL
  version don't match. Recheck Steps 1 to 3.
- **To undo everything**: run Steam's "Verify integrity of game files".

# Ark Servers

The `Server\` folder holds only what a dedicated server actually loads: the VC++ runtime files, `tbbmalloc.dll` and the EOS SDK, plus `msvcp140_atomic_wait.dll` and `msdia140.dll` for ArkAPI servers (see the Visual C++ section).

- The server doesn't use the Agility SDK, so there is nothing to patch and no `D3D12` folder to copy.
- SDL3 is not used by the client or the server.
- ArkAPI Specific - Self Hosted/VPS Servers Only (NOT RELAVENT FOR NITRADO): don't replace `libssl-3-x64.dll` / `libcrypto-3-x64.dll`. ArkAPI and its plugins are built against the OpenSSL versions they ship with, and swapping in a newer build can break them. Use the copies that come packaged with ArkAPI.

# ASA DevKit (Editor)

The `Editor` folder mirrors the DevKit install root (for example
`E:\ARKDevkit`), so its contents drop straight onto it. Close the editor
and keep backups of what you replace.

| Component | DevKit shipped | Repo | Location |
|-----------|----------------|------|----------|
| DX12 Agility | 618 | 1.620.1 | `Engine\Binaries\Win64\D3D12\x64` |
| VS Runtime | older | 14.51.36247 | `Engine\Binaries\Win64` |
| TBB (oneAPI) | older | 2023.2 | `Engine\Binaries\Win64` |
| Embree | 4.3.3 | 4.4.1 | `Engine\Binaries\Win64` |
| Python | 3.11.8 | 3.11.9 | `Engine\Binaries\ThirdParty\Python3\Win64` + `python311.dll` in `Win64` |
| Ogg / Vorbis | 1.2.2 / 1.3.2 | 1.3.6 / 1.3.7 | `Engine\Binaries\ThirdParty\Ogg` and `Vorbis\Win64\VS2015` |
| EOS SDK + XAudio2 | 1.16.2 / 1.0.0.1 | 1.19.2.1 / 1.0.2504.10003 | `Engine\Plugins\EOS\EOS\OnlineSubsystemRedpointEOS\Source\ThirdParty\Bin` |

Notes:

- **Agility SDK:** after copying, patch the editor exes the same way as the
  game: `AgilityPatch "E:\ARKDevkit\Engine\Binaries\Win64"` covers
  `ShooterGameEditor.exe`, `ShooterGameEditor-Cmd.exe` and
  `UnrealEditor-Cmd.exe`.
- **TBB:** unlike the game, the editor uses `tbb12.dll` too (Embree needs it
  for mesh builds).
- **Python:** the folder holds only the files that changed between 3.11.8
  and 3.11.9 (the official 3.11.9 Windows build). Copy the whole folder
  over; `site-packages` is not touched.
- **Ogg / Vorbis:** Xiph only ships source, so these were built from the
  upstream 1.3.6 / 1.3.7 sources with the same DLL names and exports as
  Epic's originals.
- **EOS:** the DevKit's bundled 1.16.2 SDK folder is left alone. The
  Redpoint plugin checks `Source\ThirdParty\Bin` first, so the newer SDK
  loads from there. Delete that `Source\ThirdParty` folder to go back.

# Shipped but never loaded

These ship with the game (or were in earlier versions of this repo) but nothing in the client or server ever loads them, so updating them has no effect. Checked against the exes' import tables, every DLL they load, and live module lists from a running Steam client, Game Pass client and dedicated server.

- All `boost_*-mt-x64.dll` files (left over from Epic's editor tooling)
- `tbb12.dll` (editor only)
- `concrt140.dll`, `msvcp140_2.dll`, `msvcp140_codecvt_ids.dll`, `vccorlib140.dll`, `vcruntime140_threads.dll`
- `msvcp140_atomic_wait.dll` and `msdia140.dll` on the client (ArkAPI servers only)
- `Engine\Binaries\Win64\D3D12` (the game uses the ShooterGame copy)
- `Engine\Binaries\ThirdParty\Windows\XAudio2_9\x64\xaudio2_9redist.dll` (the engine uses Windows' own XAudio2)
- `SDL3.dll`
