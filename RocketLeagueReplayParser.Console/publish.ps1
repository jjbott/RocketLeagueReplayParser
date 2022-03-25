# If its not building, try deleting publish folders. It likes to use "system" dlls from those folders sometimes.
$version = ([Xml] (Get-Content ../RocketLeagueReplayParser/RocketLeagueReplayParser.csproj)).Project.PropertyGroup.Version
Write-Host $version
Remove-Item ../publish -Recurse
dotnet publish -o ../publish/net462 -c Release -f net462
dotnet publish -o ../publish/netstandard2.0-portable -c Release -f netstandard2.0
dotnet publish -o ../publish/linux-x86 -c Release -f netstandard2.0 -r linux-x86
dotnet publish -o ../publish/linux-x64 -c Release -f netstandard2.0 -r linux-x64

Compress-Archive ../publish/net462/* "../publish/RocketLeagueReplayParser.Console.$version.zip"
Compress-Archive ../publish/netstandard2.0-portable/* "../publish/RocketLeagueReplayParser.Console.netstandard2.0.$version.zip"
Compress-Archive ../publish/linux-x86/* "../publish/RocketLeagueReplayParser.Console.linux-x86.$version.zip"
Compress-Archive ../publish/linux-x64/* "../publish/RocketLeagueReplayParser.Console.linux-x64.$version.zip"
