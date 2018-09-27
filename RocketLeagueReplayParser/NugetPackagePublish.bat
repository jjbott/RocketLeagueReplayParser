@echo off
call ..\..\SetNugetApiKey.bat
del *.nupkg
dotnet clean
dotnet pack -c Release -o .
..\..\nuget setApiKey %NUGET_API_KEY%
..\..\nuget push *.nupkg -Source nuget.org
pause