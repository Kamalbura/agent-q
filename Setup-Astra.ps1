# Setup-Astra.ps1 - first-run setup for Astra Accessibility Assistant
# - Restores NuGet packages
# - Builds solution
# - Ensures scripts are executable

param()

Write-Host "Restoring solution packages..."
dotnet restore "Assistant.sln"

Write-Host "Building solution (Debug)..."
dotnet build "Assistant.sln" -c Debug

Write-Host "Setting execution policy for current process to RemoteSigned (if allowed)"
try {
    Set-ExecutionPolicy -Scope Process -ExecutionPolicy RemoteSigned -Force
} catch {
    Write-Warning "Could not change execution policy in this environment. You can run scripts by right-click -> Run with PowerShell or change policy manually."
}

Write-Host "Setup complete. Run .\Run-Astra.ps1 to start the UI."
