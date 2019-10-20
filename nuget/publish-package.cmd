@ECHO OFF & SETLOCAL
for /f "delims=" %%F in ('dir *.nupkg /b /o-n') do nuget.cmd push %%F
pause
