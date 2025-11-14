# Astra Safety Case — Phase-1 (Summary)

Goal: present structured arguments and evidence showing that Astra is acceptably safe to run in Phase-1.

Claim 1: The system avoids executing dangerous actions without explicit confirmation.
- Argument: All LLM outputs are validated by `ActionValidator` and `SimpleSafetyLayer`. Any ambiguous UI reference sets `RequireConfirmation` and the overlay blocks execution until the user approves.
- Evidence: `Assistant.Tests/Safety/SimpleSafetyLayerTests.cs` and `Assistant.Tests/Integration/UiPlannerExecutorFlowTests.cs` verify confirmation gating.

Claim 2: The system limits blast radius of external execution.
- Argument: `IProcessLauncher` centralizes external launches and should implement sanitization and confirmation in production.
- Evidence: `ActionExecutor` uses DI to inject `IProcessLauncher`. Tests use fakes; production `ProcessLauncher` behavior is documented in `API.md`.

Claim 3: Context-aware logging preserves accountability.
- Argument: Every executed action logs screenshot hash, focused window, and target element. If a run fails safety checks, the reason is logged alongside the hash for audit replay.
- Evidence: `ActionExecutor` logging + `SimpleSafetyLayer` logging statements; verify via integration tests capturing log outputs.

Claim 4: Accessibility and safety for PWD are considered.
- Argument: Accessibility walkthrough and PWD usability scripts included. UI uses safe defaults and minimal auto-actions.
- Evidence: `docs/ACCESSIBILITY_WALKTHROUGH.md` and `docs/PWD_USABILITY_TESTS.md`.

Residual risks
- Risk: Malicious LLM prompt may attempt to craft system commands.
  - Mitigation: Add command allowlists/deny-lists and explicit confirmation dialogs before execution.

- Risk: OCR or UIA may misidentify sensitive targets.
  - Mitigation: Require user confirmation and show visual highlight of targets before executing actions that affect sensitive UI elements.
- Risk: Screenshot context may contain sensitive info in temp folder.
  - Mitigation: Hash filenames, secure storage, add scheduled cleanup in future sprint, and avoid uploading raw bytes externally.

Assurance activities (recommended next steps)
- Implement allowlist/denylist for `open_app` and document approved commands.
- Maintain the confirmation modal and extend it with screen-reader hints and highlight overlays.
- Perform adversarial prompt testing and fuzz LLM outputs in a staging environment.
