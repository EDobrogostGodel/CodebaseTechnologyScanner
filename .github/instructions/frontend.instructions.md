---
applyTo: "codebase-scanner-ui/**/*.ts,codebase-scanner-ui/**/*.tsx"
---

# Frontend Instructions

## Stack
- React 19, TypeScript (strict), Vite, MUI v6, React Router v6.
- Test runner: Vitest + `@testing-library/react`, `jsdom` environment.

## Folder responsibilities

| Folder | Purpose |
|---|---|
| `src/pages/` | Route-level components only. One component per route. |
| `src/components/` | Reusable MUI-based presentational components. |
| `src/layouts/` | `MainLayout` (Header + Outlet + Footer) and `MinimalLayout`. Do not add layouts unless a new shell is genuinely required. |
| `src/services/` | All `fetch` calls live in `scanService.ts`. |
| `src/hooks/` | Custom React hooks. `useErrorHandler` is the standard error hook. |
| `src/models/` | TypeScript interfaces that mirror backend C# DTOs exactly. |
| `src/config/` | `environment.ts` — read `VITE_*` env vars here and nowhere else. |
| `src/utils/` | Pure utility functions (`formatDate`, `formatFileSize`). |

## Routing (App.tsx)
Current routes:
- `/` — `HomePage` (inside `MainLayout`)
- `/scan` — `NewScanPage` (inside `MainLayout`)
- `/results/:scanId` — `ScanResultsPage` (inside `MainLayout`)
- `/history` — `ScanHistoryPage` (inside `MainLayout`)
- `*` — `NotFoundPage` (inside `MinimalLayout`)

Add new routes to the `MainLayout` branch unless the page truly needs no header/footer.

## API calls — `scanService`
All backend communication goes through the `scanService` object in `src/services/scanService.ts`.

```typescript
// Correct
const result = await scanService.uploadAndScan(file, projectName);

// Wrong — do not use fetch directly in pages or components
const response = await fetch('/api/scan/upload', { ... });
```

Available methods: `uploadAndScan`, `getScanResult`, `getScanHistory`, `deleteScanResult`.

The base URL is `config.apiBaseUrl` from `src/config/environment.ts` (env var `VITE_API_BASE_URL`).

### Error handling
`scanService` throws `ScanServiceError` (extends `Error`) on non-2xx responses.
- `statusCode` — HTTP status code from the API
- `details` — `ErrorResponse.details` from the API body
- `existingScanId` — set on 409 responses (`DuplicateScanErrorResponse`)

Use `useErrorHandler` in every page that makes async calls:
```typescript
const { error, handleError, clearError } = useErrorHandler('Default message');
// ...
try { ... } catch (err) { handleError(err); }
```

Handle 409 separately before calling `handleError`:
```typescript
if (err instanceof ScanServiceError && err.statusCode === 409 && err.existingScanId) {
  // show duplicate dialog
} else {
  handleError(err);
}
```

## Models (`src/models/`)
Keep interfaces in sync with the C# counterparts:

| TypeScript | C# |
|---|---|
| `ScanResult` | `ScanResult` |
| `TechnologyInfo` | `TechnologyInfo` |
| `ErrorResponse` / `DuplicateScanErrorResponse` | `ErrorResponse` / `DuplicateScanErrorResponse` |
| `PaginatedResponse` | `PaginatedResponse<ScanResult>` |
| `DeleteResponse` | `DeleteResponse` |

- Use `interface`, not `type`, for all models.
- Use named exports.
- Never use `any` — use `unknown` and narrow where needed.
- JSON field names are camelCase (ASP.NET Core default serialisation).

## Components and UI
- Use MUI components exclusively — no plain `<button>`, `<input>`, or `<table>` elements.
- No inline `style={{}}` strings — use MUI's `sx` prop.
- Components accept typed props interfaces, declared in the same file.
- Icons come from `@mui/icons-material`.

## Environment variables
Defined in `src/config/environment.ts`:
- `VITE_API_BASE_URL` — backend API base URL (default: `http://localhost:5000/api`)
- `VITE_MAX_FILE_SIZE` — max upload size in bytes (default: 52428800)

Do not read `import.meta.env` outside of `environment.ts`.

## Pagination and filtering (`ScanHistoryPage`)
- Default page size: 10 results per page.
- `sortBy` values: `"timestamp"` (default) or `"projectName"`.
- `sortOrder` values: `"asc"` or `"desc"` (default).
- Filters: `projectName` (partial match), `language`, `framework` — all optional.
- Page numbers are 1-based. Validate `page >= 1` and `pageSize` between 1–100 (enforced by the API too).
