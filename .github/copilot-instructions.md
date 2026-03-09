# GitHub Copilot Instructions — Codebase Technology Scanner

## What this app does
Accepts uploaded ZIP files, extracts them, detects programming languages (by file extension) and technologies/frameworks (by config-file name patterns), persists results to a JSON file, and exposes a REST API consumed by a React UI.

## Project layout
```
CodebaseTechnologyScanner.API/     # ASP.NET Core 8 Web API
  Controllers/                     # ScanController — thin HTTP layer only
  Services/                        # IScanService, ScanService, TechnologyDetector
  Repositories/                    # IScanRepository, FileScanRepository (JSON file)
  Models/                          # DTOs: ScanResult, TechnologyInfo, ErrorResponse, etc.
  Utils/                           # HashHelper (SHA-256), FileSizeHelper
  Program.cs                       # DI registration, CORS, Swagger
  scan-results.json                # Runtime persistence file — never edit by hand

CodebaseTechnologyScanner.Tests/   # xUnit + Moq
  Controllers/                     # ScanControllerTests
  Services/                        # ScanServiceTests, TechnologyDetectorTests
  Utils/                           # HashHelperTests, FileSizeHelperTests

codebase-scanner-ui/               # React 19 + TypeScript + Vite + MUI
  src/
    components/                    # Reusable MUI-based components (e.g., ScanResultCard)
    config/                        # environment.ts — VITE_* env vars
    hooks/                         # useErrorHandler
    layouts/                       # MainLayout, MinimalLayout
    models/                        # TypeScript interfaces mirroring API DTOs
    pages/                         # Route-level components
    services/                      # scanService (fetch wrapper + ScanServiceError)
    utils/                         # formatters (formatDate, formatFileSize)
    test/                          # Vitest setup
```

## Architecture rules to follow
- Controllers call service methods only — no business logic, no direct repository access.
- `ScanService` owns all scan orchestration: ZIP extraction, calling `TechnologyDetector`, calling the repository.
- `TechnologyDetector` is registered as `Scoped` and is **not** behind an interface; mock it via `Mock<TechnologyDetector>` in tests.
- `FileScanRepository` uses a `SemaphoreSlim(1,1)` for all write operations. Do not bypass it.
- Error responses always use `ErrorResponse { Error, StatusCode, Details }`. 409 conflicts use `DuplicateScanErrorResponse` which adds `ExistingScanId`.
- New API endpoints belong in `ScanController`. Additional controllers require a strong reason.
- Frontend API calls go through `scanService` in `src/services/scanService.ts`. Do not use `fetch` directly in components or pages.
- Frontend errors go through `useErrorHandler`. Do not manage raw `catch` error strings in component state.
- TypeScript models in `src/models/` must stay in sync with the C# model classes.

## Code style
- C#: nullable enabled, file-scoped namespaces, `string.Empty` over `""` for defaults, `ArgumentNullException.ThrowIfNull` pattern where appropriate.
- Controller actions have XML doc comments (`<summary>`, `<param>`, `<response>`) and `[ProducesResponseType]` attributes.
- TypeScript: `interface` over `type` for models, named exports, no `any`.
- React: functional components with hooks, no class components.
- MUI components only — no plain HTML `<button>` or inline style strings.

## Testing
- Backend: xUnit + Moq. Group tests in the matching `Controllers/`, `Services/`, or `Utils/` folder.
- Use Arrange/Act/Assert comments. Test class has an XML `<summary>` comment.
- Frontend: Vitest + `@testing-library/react`. Mock `scanService` with `vi.mock`. Mock `useNavigate` with `vi.fn()`.
- Every behaviour change (new feature, bug fix) must include a test.
