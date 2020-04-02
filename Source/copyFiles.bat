@echo off
SET "ProjectName=SeasonalWeather"
SET "SolutionDir=C:\Users\robin\Desktop\Games\RimWorld Modding\Source\SeasonalWeather\Source"
SET "RWModsDir=D:\SteamLibrary\steamapps\common\RimWorld\Mods"
@echo on

del /S /Q "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Defs\*"

xcopy /S /Y "%SolutionDir%\..\About\*" "%RWModsDir%\%ProjectName%\About\"
xcopy /S /Y "%SolutionDir%\..\Defs\*" "%RWModsDir%\%ProjectName%\Defs\"
xcopy /S /Y "%SolutionDir%\..\Patches\*" "%RWModsDir%\%ProjectName%\Patches\"
REM xcopy /S /Y "%SolutionDir%\..\Textures\*" "%RWModsDir%\%ProjectName%\Textures\"
xcopy /S /Y "%SolutionDir%\..\Languages\*" "%RWModsDir%\%ProjectName%\Languages\"