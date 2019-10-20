@ECHO OFF & SETLOCAL

IF NOT EXIST "%~dp0bin\nuget.exe" (
  mkdir "%~dp0bin"
  curl https://dist.nuget.org/win-x86-commandline/latest/nuget.exe --output %~dp0bin\nuget.exe
)

IF NOT EXIST "%~dp0bin\nuget.exe" (
  ECHO Failed to download nuget.exe
  EXIT 1
)

"%~dp0bin\nuget.exe" %*
