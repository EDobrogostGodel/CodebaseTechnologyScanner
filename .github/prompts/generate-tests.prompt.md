Purpose
- Generate or update unit tests for code changes in this repository using the project's existing test conventions.

Inspect first
- Open the target source file(s) under `CodebaseTechnologyScanner.API/` and any existing tests under `CodebaseTechnologyScanner.Tests/` to infer style.
- Confirm testing framework: xUnit + Moq (backend). Look for Arrange/Act/Assert comments and XML `<summary>` on test classes.

Before writing tests
- Briefly state intended coverage (one short paragraph): which class/method, which behaviours (happy path, error paths, edge cases) the tests will cover.
- Respect repository patterns: group tests into `Controllers/`, `Services/`, or `Utils/` folder mirroring the source.

Test style rules
- Use xUnit + Moq.
- Test class must have an XML `<summary>` comment.
- Use `// Arrange`, `// Act`, `// Assert` comments inside each test.
- Mock `TechnologyDetector` with `new Mock<TechnologyDetector>()` (it is not behind an interface).
- For controller tests, do not assert on framework plumbing; assert returned `ActionResult` content and status types.
- Avoid filesystem or network flakiness: mock or use temporary in-memory streams; for repository tests use a temporary file path and ensure cleanup.

What to include per target
- `ScanService`:
  - Happy path: valid ZIP stream -> extracted -> calls `TechnologyDetector.DetectTechnologiesAsync` and `AnalyzeLanguages`, saves via `IScanRepository` and returns `ScanResult` with `FileHash`.
  - Invalid ZIP: ensure exception from `ZipArchive` bubbles and is logged (assert that exception is re-thrown or handled as code expects).
  - Ensure temp directory is deleted in finally block (create a ZIP, call method, assert no leftover directory under `Path.GetTempPath()`).

- `TechnologyDetector`:
  - `AnalyzeLanguages`: given a temporary folder with files of several extensions, assert language counts match.
  - `DetectTechnologiesAsync`: with small extracted folder containing `package.json` and `.csproj`, assert frameworks and package-based detections are included; simulate malformed `package.json` to exercise logger warning path.

- `ScanController`:
  - `UploadAndScan`: happy path returns `Ok(ScanResult)`; duplicate file hash returns `Conflict(DuplicateScanErrorResponse)`; invalid file extension returns `BadRequest(ErrorResponse)`; oversized file returns `StatusCode(413)`.
  - Use in-memory `IFormFile` with `new FormFile(...)` and streams.

Test robustness
- Keep tests deterministic and non-flaky: avoid reliance on exact timestamps; assert `Timestamp` is recent within a tolerance only if necessary.
- Use `JsonSerializer` with `JsonSerializerOptions` when comparing complex objects saved to temp files.

Output
- Provide a short explanation of coverage, then the test code files ready to drop into `CodebaseTechnologyScanner.Tests/` following the repository structure. Keep tests focused and minimal to validate behaviour changes.
