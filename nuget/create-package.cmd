@ECHO OFF
del *.nupkg
..\packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe pack Caliburn.Light.nuspec -Symbols
pause
