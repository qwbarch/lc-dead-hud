@echo off

call ./build.bat
mkdir "../bin/BepInEx/plugins"
mkdir "../bin/BepInEx/plugins/DriftwoodYeet"
powershell copy "../yeet.wav" "../bin/BepInEx/plugins/DriftwoodYeet/yeet.wav"
powershell Move-Item -Path "../bin/DriftwoodYeet.dll" -Destination "../bin/BepInEx/plugins/DriftwoodYeet.dll"
powershell Compress-Archive^
    -Force^
    -Path "../bin/BepInEx",^
          "../manifest.json",^
          "../icon.png",^
          "../README.md",^
          "../LICENSE"^
    -DestinationPath "../bin/driftwood-yeet.zip"