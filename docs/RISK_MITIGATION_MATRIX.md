# Risk Mitigation Matrix — Astra Phase-1

| ID | Risk | Impact | Likelihood | Priority | Mitigation | Owner |
|----|------|--------|------------|----------|------------|-------|
| R1 | LLM returns unsafe commands | High | Medium | High | Strict validation (`ActionValidator`), confirmation UI for high-impact actions, allowlist/denylist | Product/Eng |
| R2 | OCR/UIA misidentifies UI elements | Medium | Medium | Medium | Visual confirmation, conservative defaults, fall back to manual input | Eng |
| R3 | Accessibility regressions | High | Low | Medium | Accessibility walkthroughs, automated AT tests, NVDA checks | UX/QA |
| R4 | Arbitrary process launch abuse | High | Low | High | Sanitize commands in `IProcessLauncher`, require user approval | Eng/Sec |
| R5 | CI running integration tests on headless runner fails | Low | High | Medium | Mark interactive tests as explicit; run on dedicated interactive runners | DevOps |
| R6 | Privilege escalation via launched apps | High | Low | High | Document required privileges, use least-privilege patterns, avoid elevated launches by default | Eng/Sec |

Notes
- Priority = Impact × Likelihood (qualitative). Update matrix as more data arrives.
