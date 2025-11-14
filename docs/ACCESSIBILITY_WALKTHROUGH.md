# Astra Accessibility Walkthrough (Phase-1)

Purpose: step-by-step guide to validate the Phase-1 overlay accessibility for people with disabilities (PWD), including keyboard navigation, screen reader behavior, contrast, and timing.

Scope: the WPF overlay `Assistant.UI.MainWindow` (always-on-top overlay, single-line textbox, "Listen" button, Ctrl+Space hotkey).

Quick checklist
- Keyboard-only: ensure all features reachable and operable via Tab/Shift+Tab and Enter/Space.
- Screen reader: NVDA and Narrator read labels and status changes.
- Focus visible: keyboard focus rectangle visible on controls.
- High-contrast: overlay readable in Windows high-contrast themes.
- Timeouts: ensure any blocking waits have accessible alternatives.

Walkthrough steps
1. Preparation
  - Environment: Windows 10/11 with developer build of the app running locally.
  - Tools: NVDA (free), Windows Narrator, High Contrast mode toggle, magnifier (if available).

2. Launch overlay
  - Action: Start the app (or run `dotnet run` in `Assistant.UI`).
  - Expected: Overlay window appears, topmost, with an input textbox and "Listen" button. The overlay should not steal focus unexpectedly.

3. Keyboard navigation
  - Step: Press `Tab` repeatedly until focus lands on the textbox and then on the Listen button.
  - Expected: Focus indicator visible. `Enter` on the Listen button triggers the same action as mouse click.

4. Global hotkey
  - Step: Ensure overlay can be shown/hidden with `Ctrl+Space`.
  - Expected: Pressing `Ctrl+Space` toggles overlay visibility and focus moves to the textbox.

5. Screen reader behavior
  - NVDA: Start NVDA, move to overlay, verify NVDA announces the textbox (label or placeholder) and the Listen button.
  - Narrator: Enable Narrator and repeat the check.
  - Expected: Controls are announced with helpful names (e.g., "Astra text input", "Listen button"). Status messages (for example "Listening...", "Executing plan") must be announced as they change.

6. High contrast and scaling
  - Step: Enable Windows high-contrast theme and test at 125%, 150% DPI scaling.
  - Expected: Text remains readable and controls are not truncated or clipped.

7. Timed interactions and feedback
  - Step: Trigger an action plan that includes a wait and type. Confirm there is visual and screen-reader feedback (status region) while the assistant works.
  - Expected: Status updates are accessible via live region announcements.

8. Error handling accessibility
  - Step: Provide invalid input or force a validation error from LLM stub. Confirm accessible error pop-ups or announcements.
  - Expected: Errors are spoken and visible; keyboard focus moves to the error or to an acknowledgement control.

9. Reporting issues
  - Record: OS version, overlay version (from `RELEASE_NOTES.md`), screen reader version, steps to reproduce, expected vs actual behavior, and screenshots or audio capture where helpful.

Next steps
- Add semantic AutomationProperties.Name and AutomationProperties.HelpText in `MainWindow.xaml` for all controls.
- Wire status text to an ARIA-like live region (WPF accessible live region patterns) so screen readers announce status changes.
- Add NVDA scripts to automate some checks (for CI with an accessibility harness) in later sprints.
