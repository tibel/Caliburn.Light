@ECHO OFF
del *.nupkg
..\packages\NuGet.CommandLine.2.8.5\tools\NuGet.exe pack Caliburn.Light.Annotations.nuspec
pause
