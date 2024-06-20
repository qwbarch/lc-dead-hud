@echo off

dotnet build ../src/DeadHUD.csproj
rmdir /s /q "..\bin"
mkdir "..\bin"
move "..\src\bin\Debug\netstandard2.1\DeadHUD.dll" "..\bin\DeadHUD.dll"