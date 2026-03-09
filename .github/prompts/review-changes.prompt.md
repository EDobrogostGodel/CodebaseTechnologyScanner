Purpose
- Quickly review the current changes (unstaged/staged/PR) against this repository's conventions and return a focused, actionable summary.

How to run
- Inspect the diff/changed files first.
- Load the repository conventions from `.github/copilot-instructions.md` and the backend code under `CodebaseTechnologyScanner.API/`.

What to check (in order)
1. Summary
   - One-paragraph summary of what changed and which areas (controller, service, repository, utils, UI) are affected.

2. Critical issues (must-fix before merge)
   - Breaking API behavior (controller signature/response types, changed public DTOs like `ScanResult`, `ErrorResponse`, `DuplicateScanErrorResponse`).
   - Threading/IO/synchronization problems (bypassing `FileScanRepository`'s `SemaphoreSlim`, writing `scan-results.json` concurrently, deleting temp folders without safety).
   - Security and validation regressions (file upload validation, max size config, path traversal, deserialization risks).
   - Missing or changed error response contracts or `ProducesResponseType` attributes on controller actions.

3. Important improvements (should address soon)
   - Missing null/argument checks following repository style (`ArgumentNullException.ThrowIfNull` where used elsewhere).
   - Unhandled edge-cases in scanning flow (invalid ZIPs, very large archives, nested archives, symlinks, unusual `package.json` content).
   - Logging quality: missing structured logging or swallowing exceptions without context.
   - Tests: missing or insufficient tests for new behaviour (unit tests under `CodebaseTechnologyScanner.Tests/` using xUnit + Moq; follow Arrange/Act/Assert and XML `<summary>` on test class).

4. Optional suggestions (nice-to-have)
   - Small performance improvements (avoid reading file multiple times, prefer streams where possible).
   - Improve `TechnologyDetector` detection lists to be data-driven (but only if justified).
   - Add integration/contract tests for API surface.

5. Tests & Coverage gaps
   - List changed files and for each indicate whether there is a corresponding unit test in `CodebaseTechnologyScanner.Tests/` (Controllers/Services/Utils) and which test cases are missing (happy path, negative, edge-case).

6. Risk map
   - For each changed file, score risk: Low / Medium / High and a one-line rationale.

Output format
- Keep output concise and structured with these sections: Summary, Critical issues, Important improvements, Optional suggestions, Tests & Coverage gaps, Risk map.
- Use concrete references to repository artifacts: `ScanController`, `ScanService`, `TechnologyDetector`, `FileScanRepository`, `scan-results.json`, `ErrorResponse`.

Notes
- Preserve existing architecture: controllers must remain thin, `ScanService` must own orchestration, `TechnologyDetector` is `Scoped` and should be mocked as `Mock<TechnologyDetector>` in tests.
- Prioritize minimal safe changes and call out any change that could break frontend expectations (React UI expects DTOs in `src/models/`).
