$ReleaseFolder = "release\"
dotnet build
Copy-Item bin\Debug\net48\Randomizer.dll "D:\SteamLibrary\steamapps\common\Derail Valley/Mods/Randomizer"

If (Test-Path -Path $ReleaseFolder) {
    Remove-Item $ReleaseFolder\*
} else {
    mkdir $ReleaseFolder
}

Compress-Archive bin\Debug\net48\Randomizer.dll , icons\ , Info.json , repository.json $ReleaseFolder\Randomizer.zip
Set-Location Archipelago
env\Scripts\Activate.ps1
python Launcher.py "Build APWorlds" -- "Derail Valley" --skip_open_folder
python Launcher.py "Generate Template Options" -- --skip_open_folder
Copy-Item build\apworlds\derail_valley.apworld ..\$ReleaseFolder
Copy-Item "Players\Templates\Derail Valley.yaml" ..\$ReleaseFolder