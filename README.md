# Visual Basic, FSR Upscaler, Intel Threading Building Blocks, Epic Online Services, Boost C++, Steam Redistributable and DirectX 12 to Latest Versions on PC

Ark does tend to crash alot on some systems. Outside of trying to run it on suboptimal system configurations, part of that could be the older versions of files that are currently packaged with the game. 5.7 may provide updates to these core binaries but that isn't out yet. Note: any files you copy into the game directly will likely get replaced during patches and will need re-applying afterward to maintain updated versions.

# Visual C++ - Base Game programming tasks
https://visualstudio.microsoft.com/downloads/
 
Found in \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64, can be copied from Windows/system32 and are updated via windows updates regularly with major update releases. If there is a newer version out ahead of the update schedule, those will be Included in the linked zip at the bottom with the most recent versions. But if you want to keep these up to date yourself, download visual studio and search for them with the feature updates as they are released to VS.

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

# AMD FSR UPSCALER - Resolution Scaling
Latest AMD FSR upscaler, Download the SDK package from https://gpuopen.com/fidelityfx-super-resolution-4/#downloads then in the \Kits\FidelityFX\bin folder in the zip, extract the following

- amd_fidelityfx_upscaler_dx12.dll
- amd_fidelityfx_framegeneration_dx12.dll 
- amd_fidelityfx_loader_dx12.dll 

to \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64. Delete the old amd_fidelityfx_dx12.dll and remove the loader wording in the new file so it matches the file name of the one you previously deleted. The other files you don't have to rename as the loader will use those as is. This will update it to the latest version available from AMD.

# Intel threading building blocks
tbbmalloc.dll -> https://www.intel.com/content/www/us/en/developer/tools/oneapi/onetbb-download.html
part of oneAPI
latest tbb.dll was part of Intel(R) Parallel Studio XE 2020 Update 2, 2020.3.2024.0524. Superseeded by oneAPI, needs updating....eventually. That's on WC to convert over to using oneAPI for memory management in the newer version platform.

# EPIC ONLINE SERVICES - Matchmaking, EOSID Account and Mod Authentication
https://onlineservices.epicgames.com/en-US/sdk C library. 

The EOSSDK-Win64-Shipping.dll from the SDK\Bin folder in the zip goes in the \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64\RedpointEOS.

xaudio2_9redist.dll goes in the \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64\RedpointEOS\x64
https://learn.microsoft.com/en-us/windows/win32/xaudio2/xaudio2-redistributable 
If you have visual studio installed, use the nuget package manager in a new project to install the latest Xaudio 2 package. 
Newer version of xaudio2_9redist.dll included in the attached zip link.

# STEAM REDISTRIBUTABLES - Steam Account and Mod Authentication
Steam Redistributables

- vstdlib_s.dll
- vstdlib_s64.dll
- tier0_s.dll
- tier0_s64.dll
- steamclient.dll
- steamclient64.dll 

Can be found in your steam client's root directory, updates any time the client itself is updated. Drag and drop into \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64. 

#DIRECTX 12 - Graphics API
These can also be found in your windows/system32 folder and are updated via windows updates. Place in \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64\D3D12

If you want the absolute latest and greatest function deployments then opt for the DirectX 12 Agility SDK releases instead of waiting around on the OS release schedule. https://devblogs.microsoft.com/directx/directx12agility/

IMPORTANT: IF ON WINDOWS 10 EXCLUDE COPYING THESE FROM THE ZIP AS THEY ARE NOT SUPPORTED GIVEN THEY ARE FROM A WIN11 ENVIRONMENT

- D3D12Core.dll
- dxgi.dll
- D3D12SDKLayers.dll ( this may not be in your system32 directory if you don't have visual studio installed, included in the zip )

# Boost C++: portable C++ source libraries designed to extend the functionality of the C++ programming language beyond what is provided by the C++ Standard Library.

Go to 

https://sourceforge.net/projects/boost/files/boost-binaries/ or https://boost.teeks99.com/

https://www.boost.org/releases/latest/ ( recently started posting the windows binary src but you will need to build them yourself )

Navigate to the latest major version release, under that find the latest sub version ending in -64.exe (IE 64 bit)

Run the installer, once the installer is done navigate to C:\local\boost_1_92_0 and run bootstrap.bat. That will initialize the library for windows. Now open a command prompt and type cd C:\local\boost_1_92_0 press enter. Then punch in b2 toolset=msvc link=shared stage press enter. This will initiate the library compiler to make the boost DLLs...it takes awhile to compile the library for visual studio ( which is what the game is built under ). So sit back, eat some oatmeal, grab a coffee or something while it does it's thing. When it's finished, you can move them over to replace the ones in the game directory. 

Once that is done, search for the files beginning with their corresponding counterparts below. Note, Since version 1.69, Boost.System consists entirely of inline functions and templates in the headers. It does not generate a DLL or .lib file during compilation because there is no code to compile. So you won't find it among the compiled files....don't worry about it. Copy the remaining files to the Win64 dir, delete the originals except the existing system one and rename the files to match that of the originals. Similar process of renaming much like the FSR upscaler. 

- boost_chrono-mt-x64.dll
- boost_atomic-mt-x64.dll
- boost_filesystem-mt-x64.dll
- boost_iostreams-mt-x64.dll
- boost_regex-mt-x64.dll
- boost_system-mt-x64.dll
- boost_thread-mt-x64.dll
- boost_program_options-mt-x64.dll

# Ark Servers
Goes in \ShooterGame\Binaries\Win64

SDL3.DLL - OpenGL/D3d layer
https://github.com/libsdl-org/SDL

ArkAPI Specific - Self Hosted/VPS Servers Only (NOT RELAVENT FOR NITRADO) 

- libssl-3-x64.dll - Used for SSL net traffic 
- libcrypto-3-x64.dll Used for SSL net traffic  
- msdia140.dll - Used for ARKAPI diagnostics layer

## How to Use
Download the source zip and extract all contents to \SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64 except where there are EXCEPTIONS FOR WINDOWS 10 AND DIRECT X 12!!! 

XBOX Game pass users need to be mindful that the directory structure is different from steam and note file locations, especially for EPIC's online services file. 

That's it....
