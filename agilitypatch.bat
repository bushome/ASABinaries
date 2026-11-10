@echo off
rem Edit the paths below to match your installs, then run this after copying
rem the new Agility SDK DLLs into place. Close the game/editor first.
rem Add --check after a path to only report without changing anything.

set "GAME=D:\SteamLibrary\steamapps\common\ARK Survival Ascended\ShooterGame\Binaries\Win64"
set "EDITOR=E:\ARKDevkit\Engine\Binaries\Win64"

"%~dp0AgilityPatch.exe" "%GAME%"
rem "%~dp0AgilityPatch.exe" "%EDITOR%"

pause
