@ECHO OFF
del *.nupkg
..\packages\NuGet.CommandLine.3.4.3\tools\NuGet.exe pack Caliburn.Light.nuspec -Symbols
pause
