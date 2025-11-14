param(
    [switch]$UseBuiltExe
)

# Run-Astra.ps1 - Build and run the Assistant.UI project or run the built EXE.
# Usage: .\Run-Astra.ps1  OR  .\Run-Astra.ps1 -UseBuiltExe

$solution = "Assistant.sln"
$projectPath = "Assistant.UI"
$configuration = "Debug"

Write-Host "Restoring solution..."
dotnet restore $solution

Write-Host "Building solution..."
dotnet build $solution -c $configuration

$exePath = Join-Path -Path $projectPath -ChildPath "bin\$configuration\net8.0-windows\Assistant.UI.exe"

if ($UseBuiltExe.IsPresent) {
    if (Test-Path $exePath) {
        Write-Host "Launching built EXE: $exePath"
        Start-Process -FilePath $exePath
    }
    else {
        Write-Error "Built EXE not found at $exePath. Build may have failed."
    }
}
else {
    Write-Host "Running project with 'dotnet run'"
    dotnet run --project $projectPath --configuration $configuration
}
