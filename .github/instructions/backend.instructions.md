---
applyTo: "CodebaseTechnologyScanner.API/**/*.cs,CodebaseTechnologyScanner.Tests/**/*.cs"
---

# Backend Instructions

## Project & language conventions
- Target framework: `net8.0`. Do not add or change TFM without explicit instruction.
- `<Nullable>enable</Nullable>` is active — all reference types are non-nullable unless annotated with `?`.
- Use file-scoped namespaces (`namespace Foo.Bar;`, not braced blocks).
- Use `string.Empty` instead of `""` for default/empty string values.
- Prefer `ArgumentNullException.ThrowIfNull(param)` for null guards at method entry.
- Use `var` when the type is obvious from the right-hand side; spell out the type otherwise.

## Layers and responsibilities

### Controllers (`Controllers/`)
- Thin HTTP layer only: validate inputs, call one service method, return a typed response.
- Never access `IScanRepository` or `TechnologyDetector` directly from a controller.
- Use `ILogger<T>` for error logging; log at `LogError` on caught exceptions before returning 500.
- Expose `Details` only in Development (`_environment.IsDevelopment()`).
- Every action must have:
  - XML doc comment with `<summary>`, `<param>` for each parameter, and `<response>` for each status code.
  - `[ProducesResponseType(typeof(T), statusCode)]` attributes covering all branches.

### Services (`Services/`)
- `IScanService` is the public contract; `ScanService` is the only implementation.
- `ScanService` owns: reading the ZIP stream, calling `TechnologyDetector`, calling `IScanRepository`, and returning the result.
- `TechnologyDetector` is a concrete `Scoped` service — do not introduce an interface for it.
- Always clean up temp directories in a `finally` block.
- Let exceptions propagate from `ScanService` to the controller; do not swallow them silently.

### Repository (`Repositories/`)
- `IScanRepository` defines the persistence contract; `FileScanRepository` is the only implementation.
- All write operations (`SaveScanResultAsync`, `DeleteScanResultAsync`) **must** acquire `_semaphore` before touching the file.
- Serialisation uses `System.Text.Json`. Do not introduce Newtonsoft.Json.
- The storage path comes from `IConfiguration["StorageFilePath"]`; default is `"scan-results.json"`.

### Models (`Models/`)
- `ScanResult`: `Id` (new GUID), `Timestamp` (UTC now), `ProjectName`, `Technologies`, `LanguageCounts`, `FileHash?`.
- `ErrorResponse`: `Error`, `StatusCode`, `Details?` — used for 400, 404, 413, 500.
- `DuplicateScanErrorResponse` extends `ErrorResponse` and adds `ExistingScanId` — used only for 409.
- `DeleteResponse`: `Message`, `DeletedId`.
- `PaginatedResponse<T>` wraps `Results` and a `PaginationMetadata` object.
- Do not add new models that duplicate existing ones.

### Utils (`Utils/`)
- `HashHelper.CalculateSHA256Async(Stream)` — returns a hex string; resets stream position before hashing.
- `FileSizeHelper.FormatBytes(long)` — formats bytes to a human-readable string.

## DI registrations (Program.cs)
- `IScanRepository` → `FileScanRepository` as **Singleton**.
- `TechnologyDetector` as **Scoped** (no interface).
- `IScanService` → `ScanService` as **Scoped**.
- CORS policy name is `"AllowReactApp"`.
- Swagger + XML comments enabled; XML file path derived from the executing assembly name.

## Error response contract
Always construct error responses as follows:
```csharp
// 400 / 404 / 413 / 500
return StatusCode(code, new ErrorResponse
{
    Error = "Human-readable message",
    StatusCode = code,
    Details = _environment.IsDevelopment() ? ex.Message : null  // for 500 only
});

// 409 Duplicate
return Conflict(new DuplicateScanErrorResponse
{
    Error = "Duplicate scan detected. A scan with identical content already exists.",
    StatusCode = 409,
    ExistingScanId = existingScan.Id,
    Details = $"..."
});
```

## Configuration
- `MaxUploadSizeBytes` in `appsettings.json` — validated on startup; must be between 1 KB and 1 GB.
- `StorageFilePath` in `appsettings.json` — path to the JSON persistence file.
