Purpose
- Propose and implement the smallest safe refactor for a concrete problem in this repository while preserving behaviour and public contracts.

How to start
- Open the file that appears problematic (controller, service, repository or utils) and describe the specific bug or code smell in one sentence.
- Identify the minimal surface to change (a single method, small helper, or one-file change). Do not propose cross-cutting or multi-module rewrites.

Refactor rules (must follow repository conventions)
- Preserve file-scoped namespaces, nullable annotations, and use `string.Empty` where the project uses it.
- Keep controllers thin; if the problem is in controller, prefer moving logic to `ScanService` instead of changing controller behavior.
- Reuse existing patterns: prefer `ArgumentNullException.ThrowIfNull` for parameter checks, `ILogger<T>` for logging, and `SemaphoreSlim` for repository writes.
- Do not change public DTO shapes (`ScanResult`, `ErrorResponse`, `DuplicateScanErrorResponse`) unless the change is explicit and covered by tests.
- Add or update a unit test demonstrating the bug before changing code and a second test showing the behaviour is fixed after refactor.

Deliverables
- Short problem statement (1–2 lines).
- The minimal code change (single file, or multiple small edits if absolutely necessary) with explanation why it's safe.
- One-line summary of trade-offs (e.g., small performance cost, slightly more code, improved testability).

Examples of acceptable minimal refactors
- Fix a null-check missing in `ScanService.ScanProjectAsync` by adding `ArgumentNullException.ThrowIfNull(zipStream)`.
- Ensure `FileScanRepository.WriteAllResultsAsync` uses `File.WriteAllTextAsync` with `FileOptions` if necessary to reduce race conditions — but only if you can keep `SemaphoreSlim` usage.
- Move ZIP extraction error handling into a small helper method and add a unit test for corrupted ZIP content.

Constraints
- Avoid adding new third-party dependencies.
- Keep the change reversible and well-scoped so it can be code-reviewed easily.

Output format
- Problem statement
- Proposed minimal change (file path(s) + short description)
- Rationale and trade-offs
- Tests to add/update (file paths)

Notes
- If multiple small fixes are needed, prefer producing separate minimal refactor suggestions, one per PR.