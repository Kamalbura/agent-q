# Astra Watchdog Vision Engine — Architecture & Processing

Overview
This document describes the incremental Windows screen-understanding pipeline for Astra Phase-1 and the planned components for incremental improvements.

Components (Phase-1)
- Screenshot capture (`IScreenCaptureService` + `ScreenContextCollector`) — captures primary screen bitmap, hashes it, stores temp PNG.
- OCR extraction (stub/adapter) — Phase-1 uses `NullOcrService`; pluggable once Tesseract lands.
- UIAutomation element tree (`IUiAutomationService`) — enumerates top windows + focused element; serialized into `UiElementDto` tree.
- ScreenContext DTO (`ScreenContextDto`) — immutable snapshot: screenshot path/hash/bytes, OCR text, UI tree, focused title, bounds, timestamp, and optional previous actions.
- Diff engine (`IDiffEngine`) — pixel-based comparator for change detection (future gating).
- Vision-LLM adapter (`IVisionLlmAdapter`) + `LlmPlanGenerator` — packages `{ intent, screen }`, calls the LLM, validates JSON into `AssistantAction`.
- Safety layer (`ISafetyLayer` returning `SafetyEvaluation`) — validates UI references, drops out-of-bounds clicks, marks ambiguous actions for confirmation.
- Confirmation UI — accessible dialog summarizing ambiguous actions before execution.
- Executor (`IActionExecutor`) — executes sanitized actions with context-aware validation and logging.

High-level flow (Step-by-step)
1. Capture: `IScreenCaptureService.CapturePrimaryScreen()` → Bitmap snapshot.
2. OCR: Run OCR on snapshot to produce `OcrResult` (zones, text confidence).
3. UI Tree: Query `IUiAutomationService` for focused window and relevant element tree; serialize a small JSON view.
4. Diffing: If prior snapshot exists, run `IDiffEngine.Compare(prev, current)`; if below threshold skip heavy reasoning.
5. Context assembly: `ScreenContextCollector` emits `ScreenContextDto` (ocr text, ui tree, screen bounds, screenshot hash/path, timestamp, previous actions summary).
6. Vision-LMM reasoning: `LlmPlanGenerator` serializes `{ intent, screen }` payload, calls `IVisionLlmAdapter`, validates JSON into `AssistantAction` list.
7. Safety: `ISafetyLayer.Evaluate` returns `SafetyEvaluation` with sanitized actions, confirmation requirement, and reason. Out-of-bounds `click_by_bounds` are dropped; missing UI elements block the plan.
8. Confirmation UX: If `RequiresConfirmation`, the overlay shows an accessible dialog listing ambiguous elements (names + context). User approves/denies via keyboard.
9. Execution: Confirmed actions flow into `IActionExecutor.ExecutePlanAsync(actions, context)` which revalidates context links, logs, and performs input automation.

Processing logic details
- Capture cadence: default 0.5–2s depending on user-configured polling and CPU budget. Use diff threshold to avoid unnecessary LLM calls.
- OCR sizing: scale down to 1/2 for OCR to reduce latency; keep 300 DPI crops for text-dense regions.
- UI tree pruning: only include top-level window, focused element, and unique labels (max 20 nodes). Limit recursion depth.
- Privacy: redact fields marked as sensitive (password fields, secured text) before sending to LLM.

Testable behavior rules
- R1: If diff_score < 0.01 and no focus change, skip LLM call.
- R2: LLM must return valid JSON; otherwise, retry up to N=1 with a stricter prompt and then stop.
- R3: Any `open_app` target must be in the allowlist maintained by `ISafetyLayer`.
- R4: Ambiguous UI names (multiple matches) force `RequireConfirmation=true` and must surface in the dialog.
- R5: `click_by_bounds` outside `ScreenContextDto.ScreenBounds` are dropped before execution.
- R6: Executor refuses to run actions still marked `RequireConfirmation` or referencing unknown `UiElementId`s.

Prompting instructions for the agent-brain LLM
- System instruction (concise): "You are Astra's Vision Assistant. Given screen context (OCR + UI tree + metadata), produce a JSON array of actions following the schema: [{type: 'open_app'|'type_text'|'wait'|'click_by_bounds', args:{target?:string, text?:string, delayMs?:int, uiElementId?:string, bounds?:{x,y,width,height}, requireConfirmation?:bool, metadata?:object}}].
   Only output valid JSON and nothing else. When uncertain, set args.requireConfirmation=true and describe the ambiguity in metadata."

- Example prompt wrapper:
```
SYSTEM: You are Astra Vision Assistant. OUTPUT MUST BE STRICT JSON array matching the Action schema. Never include commentary.

USER: Here is the screen context:
{context_json}

TASK: Identify the smallest set of actions to fulfill: "{user_goal}". Only produce actions if you are >90% confident. If not, output: [{"type":"confirm","args":{"text":"I'm not sure — confirm action to open X"}}]
```

Safety & privacy considerations
- Do not send raw screenshots to external LLMs in Phase-1. Prefer local LLMs or summarize OCR/UIA text only.
- Mark and redact sensitive fields (password, SSNs) before sending.
- Confirmation dialog is mandatory for any action flagged by safety (e.g., ambiguous names, low OCR confidence). Logs capture screenshot hash + decision for auditing.

Next steps / roadmap
- Sprint A: Add `TesseractOcrService` integration and unit tests with sample images.
- Sprint B: Implement region-of-interest detection and prioritized OCR for dynamic controls.
- Sprint C: Add Vision-LMM adapter for an on-device LLM (Ollama or private model) and comprehensive adversarial testing.
- Sprint D: Introduce virtual display (off-screen buffer) for proactive planning without intrusively capturing the real desktop.
