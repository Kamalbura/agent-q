# Astra API Reference (Phase-1)

This document describes the public types, interfaces and contracts used in the Phase-1 codebase.

Core Models
- `Assistant.Core.Actions.AssistantAction`
  - Properties: `string Type`, `ActionArgs Args`
  - Purpose: represents a single action the assistant can execute.
- `Assistant.Core.Actions.ActionArgs`
  - Properties: `string Target`, `string Text`, `int? DelayMs`
  - Purpose: carries parameters for action types like `open_app`, `type_text`, and `wait`.

Interfaces
- `IActionExecutor` (Assistant.Executor.Abstractions)
  - Methods:
    - `Task ExecutePlanAsync(string actionPlanJson)` — execute a JSON plan end-to-end.
    - `bool TryParseActions(string json, out IReadOnlyList<AssistantAction> actions, out string error)` — parse and validate plan.
  - Thread-safety: implementation should be safe to call from background threads but UI updates must be marshalled to UI thread.

- `ITextEntrySimulator` (Assistant.Executor.Abstractions)
  - Methods: `void EnterText(string text)` — inject text keystrokes.
  - Note: production implementations use `WindowsInput.Simulate`.

- `IProcessLauncher` (Assistant.Executor.Abstractions)
  - Methods: `void Launch(string command)` — start an external application or open a document.

- `ILlmClient` (Assistant.LLM)
  - Methods: `Task<string> GetActionPlanAsync(string prompt)` — returns an action plan as JSON.
  - Phase-1: implementation `OllamaClientStub` returns a sample JSON payload.

- `IOcrService` / `IUiAutomationService` (Assistant.Core.Services)
  - Purpose: provide optional screen context to the LLM. Phase-1 includes safe stubs.

Serialization
- All JSON uses `System.Text.Json` with shared options (`JsonSettings`) configured for camelCase and to ignore nulls.

Validation
- Use `Assistant.Core.Validation.ActionValidator.TryValidate(json, out actions, out errors)` to ensure LLM output is strictly validated before execution.

Errors and failures
- Implementations must not execute actions when validation fails. Errors should be surfaced to the UI and logged.

Security
- Never execute unknown or user-provided shell commands without explicit confirmation. The `IProcessLauncher` contract expects a sanitized command.
