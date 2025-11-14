# PWD Usability Testing Scripts — Astra Phase-1

Purpose: scripted scenarios and metrics to validate usability for people with disabilities (PWD).

Test sessions: perform with a facilitator and one test participant who uses assistive tech (AT) they normally use.

Metrics to capture
- Task completion (Success / Partial / Fail)
- Time to complete task
- Number of assistive tech interruptions or recoveries
- Participant subjective difficulty (1-7 Likert)

Core tasks
1. Launch and open overlay (Keyboard only)
  - Goal: show overlay and focus on textbox using `Ctrl+Space`.
  - Success: overlay visible and textbox focused within 5s.

2. Enter text and run (Screen reader)
  - Goal: type a short command and press Listen. Screen reader should announce status changes.
  - Success: participant hears "Listening" and later "Executing plan".

3. Cancel or dismiss overlay (Keyboard)
  - Goal: close overlay with keyboard without closing host app.
  - Success: overlay hidden; app remains running.

4. Error recovery
  - Goal: trigger a known invalid plan (LLM stub returns bad JSON) and verify participant can read/acknowledge error.
  - Success: error announced and focus moves to acknowledgement control.

Facilitator notes
- Record AT setup, screen reader version, and magnifier scale.
- Note any confusing phrasing in status messages.

Consent and safety
- Obtain consent to record audio or video; redact personal data from logs.

Reporting
- Use spreadsheet rows for each participant capturing metrics and notes.
