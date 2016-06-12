@echo off
call ..\..\SetNugetApiKey.bat
del *.nupkg
..\..\nuget pack RocketLeagueReplayParser.csproj -Build -Prop Configuration=Release
..\..\nuget setApiKey %NUGET_API_KEY%
..\..\nuget push *.nupkg
pause