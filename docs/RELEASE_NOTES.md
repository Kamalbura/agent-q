# Astra — Release Notes (Phase-1)

Version: 0.1.0 (Phase-1 MVP)
Date: 2025-11-14

Highlights
- WPF overlay with always-on-top behavior, textbox input and "Listen" button.
- Global hotkey registered (Ctrl+Space) to toggle overlay visibility.
- Action schema (`open_app`, `type_text`, `wait`) and JSON validation implemented.
- Executor skeleton implemented with DI-friendly `ITextEntrySimulator` and `IProcessLauncher`.
- LLM adapter stub (OllamaClientStub) returning a sample action plan.
- Unit and integration tests for validation, parsing, and executor delegation.
- Safety-first stubs for OCR/UIA and accessibility checklist.

Known issues
- OCR and real LLM adapter not integrated — current LLM is a stub.
- Some integration tests require an interactive desktop and are marked accordingly.

Upgrade notes
- When upgrading to a production LLM adapter, make sure to add strict validation flows and a confirmation step for any potentially destructive action.

How to get the build
1. Restore: `dotnet restore`
2. Build: `dotnet build Assistant.sln`
3. Test: `dotnet test Assistant.sln`

Contact
- Report issues or request features by opening an issue in the repository.
