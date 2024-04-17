dotnet publish /p:Configuration=Release /p:PublishProfile=PublishLinuxX64Net8
dotnet publish /p:Configuration=Release /p:PublishProfile=PublishLinuxX64Selfcontained
dotnet publish /p:Configuration=Release /p:PublishProfile=PublishWindowsX64Net8
dotnet publish /p:Configuration=Release /p:PublishProfile=PublishWindowsX64Selfcontained
Push-Location "publish"
Get-ChildItem -Directory | ForEach-Object { 
    Compress-Archive -Path "$($_.FullName)\*" -DestinationPath "AvaloniaTFTPClient_$($_.Name).zip" -CompressionLevel Optimal -Force 
}
Pop-Location