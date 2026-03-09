---
applyTo: "CodebaseTechnologyScanner.Tests/**/*.cs,codebase-scanner-ui/src/**/*.test.ts,codebase-scanner-ui/src/**/*.test.tsx"
---

# Tests Instructions

## Rule: every behaviour change needs a test
Adding a feature, fixing a bug, or changing a response shape requires a matching test. No exceptions.

---

## Backend — xUnit + Moq

### File placement
Mirror the API project structure exactly:

| API code | Test file |
|---|---|
| `Controllers/ScanController.cs` | `Controllers/ScanControllerTests.cs` |
| `Services/ScanService.cs` | `Services/ScanServiceTests.cs` |
| `Services/TechnologyDetector.cs` | `Services/TechnologyDetectorTests.cs` |
| `Utils/HashHelper.cs` | `Utils/HashHelperTests.cs` |
| `Utils/FileSizeHelper.cs` | `Utils/FileSizeHelperTests.cs` |

### Class structure
```csharp
/// <summary>
/// Unit tests for XyzService.
/// One-line description of what is covered.
/// </summary>
public class XyzServiceTests
{
    // Mocks and SUT declared as fields
    private readonly Mock<IScanRepository> _repositoryMock;
    private readonly XyzService _service;

    public XyzServiceTests()
    {
        _repositoryMock = new Mock<IScanRepository>();
        _service = new XyzService(_repositoryMock.Object);
    }

    [Fact]
    public async Task MethodName_Condition_ExpectedBehaviour()
    {
        // Arrange
        ...
        // Act
        var result = await _service.MethodAsync(...);
        // Assert
        Assert.Equal(expected, result);
    }
}
```

### Mocking rules
- `IScanService` — `Mock<IScanService>` (in controller tests).
- `IScanRepository` — `Mock<IScanRepository>` (in service tests).
- `TechnologyDetector` — `Mock<TechnologyDetector>(detectorLoggerMock.Object)` (concrete class, must pass its `ILogger<TechnologyDetector>` constructor arg).
- `ILogger<T>` — always mock; never assert on log calls unless the test is specifically about logging.
- `IConfiguration` — `Mock<IConfiguration>`; set values via `.Setup(c => c.GetSection("Key").Value).Returns("value")`.
- `IWebHostEnvironment` — `Mock<IWebHostEnvironment>`; set `EnvironmentName` to `"Development"` by default.

### Controller test setup
```csharp
_controller = new ScanController(
    _serviceMock.Object,
    _loggerMock.Object,
    _configMock.Object,
    _environmentMock.Object);
```
Assert on the concrete result type first, then the response body type:
```csharp
var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
var body = Assert.IsType<ErrorResponse>(actionResult.Value);
Assert.Equal(400, body.StatusCode);
```
For 409, assert on `DuplicateScanErrorResponse` and verify `ExistingScanId` is populated.

### What to test
- Happy path: correct return type and content.
- Not-found path: `null` from repository → correct 404 / `null` service return.
- Validation branches: missing file, wrong extension, oversized file.
- Duplicate detection: service returns an existing `ScanResult` → 409 with `ExistingScanId`.
- Pagination: correct slicing, page metadata (`TotalPages`, `HasNextPage`, `HasPreviousPage`).
- Filtering: by `projectName`, `language`, `framework`; case-insensitive.
- Sorting: `timestamp` asc/desc, `projectName` asc/desc.
- Delete: found → 200; not found → 404.

---

## Frontend — Vitest + @testing-library/react

### File placement
Co-locate test files with their source or use the same sub-folder convention:
- `src/pages/NewScanPage.test.tsx` (already exists — follow its pattern)
- `src/hooks/useErrorHandler.test.ts` (already exists — follow its pattern)
- `src/utils/formatters.test.ts` (already exists — follow its pattern)

### Mock pattern for `scanService`
```typescript
vi.mock('../../services/scanService', () => ({
  scanService: {
    uploadAndScan: vi.fn(),
    getScanHistory: vi.fn(),
    getScanResult: vi.fn(),
    deleteScanResult: vi.fn(),
  },
  ScanServiceError: class ScanServiceError extends Error {
    statusCode: number;
    details?: string;
    existingScanId?: string;
    constructor(message: string, statusCode: number, details?: string, existingScanId?: string) {
      super(message);
      this.statusCode = statusCode;
      this.details = details;
      this.existingScanId = existingScanId;
    }
  },
}));
```

### Mock pattern for React Router
```typescript
const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return { ...actual, useNavigate: () => mockNavigate };
});
```

### Render helper
Wrap components that use routing in `<BrowserRouter>`:
```typescript
render(
  <BrowserRouter>
    <ComponentUnderTest />
  </BrowserRouter>
);
```

### What to test (pages)
- Renders expected heading and key controls.
- Submit is disabled until required fields are filled.
- Successful async call → `navigate` called with the correct path.
- API error → error message visible in the DOM.
- 409 response → duplicate dialog shown with a link to the existing scan.
- Delete confirmation dialog: cancel keeps item, confirm calls `deleteScanResult`.

### Assertions
- Use `screen.getByText`, `screen.getByRole`, `screen.getByLabelText` — prefer roles and labels over test IDs.
- Use `waitFor` / `findBy*` for async DOM updates.
- Check `mockNavigate` was called with the exact route string.
- Reset mocks in `beforeEach(() => vi.clearAllMocks())`.

### Test structure
```typescript
describe('ComponentName', () => {
  beforeEach(() => { vi.clearAllMocks(); });

  it('renders the upload form', () => { ... });
  it('shows error when no file is selected on submit', async () => { ... });
  it('navigates to results page on successful scan', async () => { ... });
});
```
