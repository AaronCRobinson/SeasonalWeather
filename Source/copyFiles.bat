@echo off
SET "ProjectName=SeasonalWeather"
SET "SolutionDir=C:\Users\robin\Desktop\Games\RimWorld Modding\Source\SeasonalWeather\Source"
@echo on

del /S /Q "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Defs\*"

xcopy /S /Y "%SolutionDir%\..\About\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\About\"
xcopy /S /Y "%SolutionDir%\..\Defs\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Defs\"
xcopy /S /Y "%SolutionDir%\..\Patches\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Patches\"
REM xcopy /S /Y "%SolutionDir%\..\Textures\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Textures\"
xcopy /S /Y "%SolutionDir%\..\Languages\*" "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\Mods\%ProjectName%\Languages\"