@ECHO OFF
del *.nupkg
..\packages\NuGet.CommandLine.3.3.0\tools\NuGet.exe pack Caliburn.Light.Annotations.nuspec
pause
