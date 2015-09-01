@ECHO OFF
for /f "delims=" %%F in ('dir *.nupkg /b /o-n') do set PACKAGE=%%F
..\packages\NuGet.CommandLine.2.8.6\tools\NuGet.exe push %PACKAGE%
pause
