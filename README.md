# Astra Assistant (Phase 1)

Astra is a Windows accessibility overlay that stays on top of the desktop, listens for user prompts, requests structured actions from an LLM adapter, and executes those actions locally.

## Solution Layout

- `Assistant.UI` – WPF overlay with Ctrl+Space hotkey, DI wiring, context capture, and confirmation dialog.
- `Assistant.Core` – Action schema, `ScreenContextDto`, validators, and shared contracts.
- `Assistant.Executor` – Screen capture, OCR/UIA stubs, planner, safety layer, and executor implementations.
- `Assistant.LLM` – Vision LLM stub adapter returning deterministic sample plans.
- `Assistant.Tests` – NUnit unit/integration tests for validation, safety, diffing, screen context, and pipeline flow.

## Prerequisites

- Windows 10/11
- .NET 8 SDK (`winget install Microsoft.DotNet.SDK.8`) 

## Build and Test

```powershell
cd C:\Users\burak\ptojects\agent-q
dotnet restore Assistant.sln
dotnet build Assistant.sln
dotnet test Assistant.sln
```

## Run the Overlay

```powershell
cd C:\Users\burak\ptojects\agent-q
dotnet run --project Assistant.UI\Assistant.UI.csproj
```

- Ctrl+Space toggles the always-on-top window.
- Enter a prompt and press **Listen** to capture the current screen, build a vision-aware payload, run the stub planning pipeline, approve ambiguous actions, and execute the safe subset locally.

### Integration/Pipeline Tests

```powershell
dotnet test Assistant.Tests/Assistant.Tests.csproj --filter "Category=Integration"
```

The integration suite exercises context capture, safety gating, and executor flows using deterministic stubs.

## Configuring Real LLM Access

The `Assistant.LLM` project currently returns mocked plans. When integrating with a live endpoint:

1. Add secure storage for API keys (environment variables or a protected secrets file ignored by git).
2. Replace `OllamaClientStub` with a concrete client that posts the transcript and parses the response into the existing action schema.
3. Ensure the new client continues to emit well-formed JSON validated by `ActionValidator` and compatible with the extended action schema (UI references, bounds, confirmation flags).

## Continuous Integration

`.github/workflows/ci.yml` runs `dotnet build` and `dotnet test` on every pull request using Windows runners, keeping the WPF overlay healthy.

## Run Scripts (Windows)

To simplify first-run and development, the repository includes helper scripts in the repo root:

- `Setup-Astra.ps1` — first-run setup: restores packages and builds the solution.
- `Run-Astra.ps1` — builds and runs the `Assistant.UI` project (or runs the built EXE with `-UseBuiltExe`).
- `RunAstra.bat` — Windows batch wrapper that runs `dotnet run` for the UI.

Examples:

PowerShell (build & run):
```powershell
.\Setup-Astra.ps1
.\Run-Astra.ps1
```

Run the already-built EXE:
```powershell
.\Run-Astra.ps1 -UseBuiltExe
```

Or with CMD:
```cmd
RunAstra.bat
```
