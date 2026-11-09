# Visual Basic, FSR Upscaler, Intel Threading Building Blocks, Epic Online Services, Boost C++, Steam Redistributable and DirectX 12 to Latest Versions on PC

Ark does tend to crash alot on some systems. Outside of trying to run it on suboptimal system configurations, part of that could be the older versions of files that are currently packaged with the game. Note: any files you copy into the game directly will likely get replaced during patches and will need re-applying afterward to maintain updated versions.

So at a glance what this Repo covers in terms of updates to the shipped binaries

| Component     | Shipped       | current       | Gap            |
|---------------|---------------|---------------|----------------|
| TBB (OneAPI)  | 2022.3.0.0    | 2023.2.0.0    | ~1 year        |
| DX12 Agility  | 618           | 620           | 1.1 years      |
| XAudio2       | 1.0.0.1       | 1.0.2504.10003| ~13 years?!    |
| EOS SDK       | 1.16.2.0      | 1.19.2.1      | 2.5 years      |
| Boost         | 1.82          | 1.92          | 3.4 years      | 
| VS Runtime    | 14.50.35719   | 14.51.36247   | 6.5 months     |    

# Visual C++ - Base Game programming tasks
https://visualstudio.microsoft.com/downloads/
 
If you want to keep these up to date yourself on the latest stable release, download visual studio and search for them with the feature updates as they are released to VS. They will usually reside in C:\Program Files\Microsoft Visual Studio\18\Community\VC\Redist\MSVC\14.51.36231\x64\Microsoft.VC145.CRT while msdia140.dll will be in C:\Program Files\Microsoft Visual Studio\18\Community\DIA SDK\bin\amd64

- concrt140.dll
- msdia140.dll
- msvcp140.dll
- msvcp140_1.dll
- msvcp140_2.dll
- msvcp140_atomic_wait.dll
- msvcp140_codecvt_ids.dll
- vccorlib140.dll
- vcruntime140.dll
- vcruntime140_1.dll
- vcruntime140_threads.dll

# Intel threading building blocks
tbb12.dll & tbbmalloc.dll -> https://www.intel.com/content/www/us/en/developer/tools/oneapi/onetbb-download.html
part of oneAPI 

IMPORTANT -> The old tbb.dll that had been shipping with ark ascended since release no longer applies and MUST be removed post UE5.8 as of 11/6/2026. 

# EPIC ONLINE SERVICES - Matchmaking, EOSID Account and Mod Authentication
https://onlineservices.epicgames.com/en-US/sdk C library. 

For steam, the EOSSDK-Win64-Shipping.dll from the SDK\Bin folder goes in the \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64\RedpointEOS.

For Microsoft Gamepass users, place this under \XboxGames\ARK- Survival Ascended\Content\ShooterGame\Binaries\WinGDK

xaudio2_9redist.dll goes in the \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64\RedpointEOS\x64
https://learn.microsoft.com/en-us/windows/win32/xaudio2/xaudio2-redistributable 
If you have visual studio installed, use the nuget package manager in a new project to install the latest Xaudio 2 package. 

# Using a newer Direct X 12 Agility SDK (patching the game exe) 

The game exe stores the Agility SDK version it expects. If the DLLs in the
`D3D12` folders are a different version, the game fails at launch with
"DirectX 12 is not supported on your system". To use newer DLLs you must
change that stored number to match. THERE ARE TWO FOLDER LOCATIONS THESE ARE STORED
IN. THE ENGINE PARENT FOLDER, AND THE SHOOTERGAME PARENT FOLDER, EACH WITH THEIR 
OWN BINARIES/WIN64 SUB DIRECTORIES. IF YOU ARE NOT GOING TO BE USING THE NEWER 
AGILITY SDK FILES IN THE REPO WITH THIS THEN DO NOT DROP THEM INTO YOUR GAME
FOLDER!!!! CHOOSE ONLY THE FILES YOU ACTUALLY NEED!!!!!!!!!!

> **Warning**
> - This modifies `ArkAscended.exe`. **Do not use it with BattlEye.** Only
>   launch without BattlEye and only join servers that don't enforce it.
> - Steam's "Verify integrity of game files" and every game update will undo
>   the patch. Repeat these steps after each update.
> - Keep a copy of the original exe before you start.

I have included a simple patcher and bat file to simplify the work below. All you
need to do is modify the bat file to have the correct path for your ark ascended
installation and the correct MAJOR version number of the Agility version you are
setting the game executable to. The number in the provided bat file will normally
match the same version that is included in this repo and the resultant output will 
have a name of the d3d version appended to the exe name so you can backup your 
original exe before using the patched one. 

### What you need

- [CFF Explorer](https://ntcore.com/explorer-suite/) (part of Explorer Suite)
- The `D3D12Core.dll` and `d3d12SDKLayers.dll` from this repo

### Step 1: Find your two version numbers

You need two numbers: the version your game **currently ships** and the
**new** version you are installing. In both cases the number is the
**middle part** of the SDK version (for example, 1.619.6 gives 619).

1. **Current version:** before replacing anything, right-click the original
   `D3D12Core.dll` in the game's `D3D12` folder > Properties > Details, and
   read the product version.
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
originals in the game's `D3D12` folder.

### Troubleshooting

- **"DirectX 12 is not supported"**: the number in the exe and the DLL
  version don't match. Recheck Steps 1 to 3.
- **To undo everything**: run Steam's "Verify integrity of game files".

# Boost C++: portable C++ source libraries designed to extend the functionality of the C++ programming language beyond what is provided by the C++ Standard Library.

Go to 

https://sourceforge.net/projects/boost/files/boost-binaries/ or https://boost.teeks99.com/

https://www.boost.org/releases/latest/ ( recently started posting the windows binary src but you will need to build them yourself )

Navigate to the latest major version release, under that find the latest sub version ending in -64.exe (IE 64 bit)

Run the installer, once the installer is done navigate to C:\local\boost_1_92_0 and run bootstrap.bat. That will initialize the library for windows. Now open a command prompt and type cd C:\local\boost_1_92_0 press enter. Then punch in b2 toolset=msvc link=shared stage press enter. This will initiate the library compiler to make the boost DLLs...it takes awhile to compile the library for visual studio ( which is what the game is built under ). So sit back, eat some oatmeal, grab a coffee or something while it does it's thing. 

Once that is done, search for the files beginning with their corresponding counterparts below. Note, Since version 1.69, Boost.System consists entirely of inline functions and templates in the headers. It does not generate a DLL or .lib file during compilation because there is no code to compile. So you won't find it among the compiled files....don't worry about it. Copy the remaining files from the stage folder to the Win64 dir, delete the originals except the existing system one and rename the files to match that of the originals.

- boost_atomic-mt-x64.dll
- boost_chrono-mt-x64.dll
- boost_container-mt-x64.dll
- boost_filesystem-mt-x64.dll
- boost_graph-mt-x64.dll
- boost_iostreams-mt-x64.dll
- boost_program_options-mt-x64.dll
- boost_random-mt-x64.dll
- boost_regex-mt-x64.dll
- boost_thread-mt-x64.dll

# Ark Servers
Goes in \ShooterGame\Binaries\Win64

SDL3.DLL - SDL handles windows, keyboard, mouse and controller input, and audio across platforms.
https://github.com/libsdl-org/SDL

ArkAPI Specific - Self Hosted/VPS Servers Only (NOT RELAVENT FOR NITRADO) 

- libssl-3-x64.dll - Used for SSL net traffic 
- libcrypto-3-x64.dll Used for SSL net traffic  

## How to Use

The folder structures are setup just like the ones in the client or server directory so you can drop them right on top of the root folder. REMEMBER, DO NOT COPY OVER YOUR DIRECT X 12 VERSION FOLDERS IF YOU HAVE NO INTENTION OF PLAYING THE GAME ON A MODIFIED EXECUTABLE TO COVER THE NEW VERSION. YOU WILL BE UNABLE TO JOIN ANY SERVERS WITH BATTLE EYE ENABLED.
