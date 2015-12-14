@ECHO OFF
for /f "delims=" %%F in ('dir *.nupkg /b /o-n') do set PACKAGE=%%F
..\packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe push %PACKAGE%
pause
