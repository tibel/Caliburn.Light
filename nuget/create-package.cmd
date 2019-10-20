@ECHO OFF & SETLOCAL
del *.nupkg
nuget.cmd pack Caliburn.Light.nuspec -Symbols
pause
