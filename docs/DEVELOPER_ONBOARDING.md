# Developer Onboarding — Astra (Phase-1)

Prerequisites
- Windows 10/11 (desktop) with .NET 8 SDK installed.
- Visual Studio 2022/2023 or VS Code with C# extension.

Clone and build
```powershell
git clone <repo-url>
cd agent-q
dotnet restore
dotnet build Assistant.sln
dotnet test Assistant.sln
```

Project layout
- `Assistant.UI` — WPF overlay and entry point.
- `Assistant.Core` — data models, validation and shared contracts.
- `Assistant.Executor` — implementations for executing actions.
- `Assistant.LLM` — LLM adapter stubs.
- `Assistant.Tests` — unit and integration tests.

Running locally
- Launch overlay (UI project):
```powershell
cd Assistant.UI
dotnet run
```

Testing
- Run tests for solution: `dotnet test Assistant.sln`
- Integration tests that need an interactive desktop are marked with `[Category("Integration")]` and may be run selectively.

Adding dependencies
- Use `dotnet add <project> package <package>` and update repository README when adding native dependencies (e.g., Tesseract).

Contributing
- Follow the existing coding style and keep changes minimal and focused.
- Add unit tests for new logic; Prefer dependency injection and small interfaces for testability.

Security & Safety notes
- Do not wire any production LLM keys into the repo. Use secrets managers.
- Confirm any action that may modify files, system state or launch external processes.
