# Codebase Technology Scanner

A full-stack web application that scans project folders (uploaded as ZIP files) and detects programming languages, frameworks, tools, and libraries used in the project.

## Features

### Core Functionality
- [x] ZIP file upload and scanning
- [x] Technology detection (languages, frameworks, tools)
- [x] **Duplicate scan detection** - Prevents re-scanning identical projects
- [x] **Pagination** - Browse through scan history efficiently
- [x] **Filtering** - Filter scans by project name, language, or framework
- [x] **Sorting** - Sort by date or project name
- [x] **Delete scans** - Remove unwanted scan results
- [x] Persistent storage - Data survives server restarts
- [x] Configurable file size limits

## Prerequisites

Before you begin, ensure you have the following installed:
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 18+** and **npm** - [Download here](https://nodejs.org/)
- **Modern web browser** (Chrome, Firefox, Edge, Safari)
- **Git** (optional, for cloning the repository)

## Architecture

### Backend (.NET 8 Web API)
- **Technology**: ASP.NET Core 8.0
- **Storage**: File-based JSON storage (scan-results.json)
- **Features**:
  - ZIP file upload and extraction
  - SHA256 hash-based duplicate detection
  - Technology detection from file extensions and configuration files
  - Language analysis
  - Framework and library detection (npm, NuGet, etc.)
  - RESTful API endpoints with pagination and filtering
  - Standardized error responses
  - CORS enabled for React frontend

### Frontend (React + TypeScript)
- **Technology**: React 19, TypeScript, Vite
- **UI Library**: Material-UI (MUI)
- **Routing**: React Router v7
- **Features**:
  - File upload interface with size display
  - Duplicate scan detection dialog
  - Scan results visualization
  - Scan history management with pagination
  - Advanced filtering and sorting
  - Delete functionality with confirmation dialogs
  - Responsive Material Design

## Project Structure

```
CopilotCourse/
├── CodebaseTechnologyScanner.API/     # Backend .NET Web API
│   ├── Controllers/
│   │   └── ScanController.cs
│   ├── Services/
│   │   ├── IScanService.cs
│   │   ├── ScanService.cs
│   │   └── TechnologyDetector.cs
│   ├── Repositories/
│   │   ├── IScanRepository.cs
│   │   └── FileScanRepository.cs
│   ├── Models/
│   │   ├── ScanRequest.cs
│   │   ├── ScanResult.cs
│   │   ├── TechnologyInfo.cs
│   │   ├── ErrorResponse.cs
│   │   ├── PaginatedResponse.cs
│   │   └── DeleteResponse.cs
│   ├── Utils/
│   │   ├── FileSizeHelper.cs
│   │   └── HashHelper.cs
│   ├── Program.cs
│   └── appsettings.json
│
└── codebase-scanner-ui/               # Frontend React App
    ├── src/
    │   ├── components/
    │   │   ├── Header.tsx
    │   │   ├── Footer.tsx
    │   │   └── ScanResultCard.tsx
    │   ├── layouts/
    │   │   └── MainLayout.tsx
    │   ├── pages/
    │   │   ├── HomePage.tsx
    │   │   ├── NewScanPage.tsx
    │   │   ├── ScanResultsPage.tsx
    │   │   └── ScanHistoryPage.tsx
    │   ├── services/
    │   │   └── scanService.ts
    │   ├── models/
    │   │   ├── ScanRequest.ts
    │   │   ├── ScanResult.ts
    │   │   ├── TechnologyInfo.ts
    │   │   ├── ErrorResponse.ts
    │   │   ├── PaginatedResponse.ts
    │   │   └── DeleteResponse.ts
    │   ├── App.tsx
    │   └── main.tsx
    └── package.json
```

## Pages

1. **Home Page** (`/`) - Landing page with application overview
2. **New Scan Page** (`/scan`) - Upload ZIP file and initiate scan
3. **Scan Results Page** (`/results/:id`) - View detailed scan results
4. **Scan History Page** (`/history`) - Browse, filter, sort, and manage past scans

## Getting Started

### Backend Setup

1. Navigate to the API directory:
   ```bash
   cd CodebaseTechnologyScanner.API
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. (Optional) Update `appsettings.json` to customize settings:
   ```json
   {
     "StorageFilePath": "scan-results.json",
     "MaxUploadSizeBytes": 52428800
   }
   ```

4. Run the API:
   ```bash
   dotnet run
   ```

   The API will start on:
   - HTTP: `http://localhost:5000`
   - HTTPS: `https://localhost:7094`
   - Swagger UI: `http://localhost:5000/swagger`

### Frontend Setup

1. Navigate to the UI directory:
   ```bash
   cd codebase-scanner-ui
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. (Optional) Update API URL if needed in `src/services/scanService.ts`:
   ```typescript
   const API_BASE_URL = 'http://localhost:5000/api';
   ```

4. Run the development server:
   ```bash
   npm run dev
   ```

   The app will start on `http://localhost:5173`

## API Endpoints

### Scan Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/scan` | Upload and scan a ZIP file |
| `GET` | `/api/scan/{id}` | Get a specific scan by ID |
| `GET` | `/api/scan/history` | Get paginated scan history with filtering and sorting |
| `DELETE` | `/api/scan/{id}` | Delete a scan by ID |

### Query Parameters (for `/api/scan/history`)

- `page` - Page number (default: 1)
- `pageSize` - Items per page (default: 20, max: 100)
- `sortBy` - Sort field: `timestamp` or `projectName` (default: `timestamp`)
- `sortOrder` - Sort order: `asc` or `desc` (default: `desc`)
- `projectName` - Filter by project name (partial, case-insensitive)
- `language` - Filter by detected language
- `framework` - Filter by detected framework

### Example Requests

**Scan a project:**
```bash
curl -X POST http://localhost:5000/api/scan \
  -F "zipFile=@project.zip" \
  -F "projectName=My Project"
```

**Get scan history (filtered and sorted):**
```bash
curl "http://localhost:5000/api/scan/history?page=1&pageSize=20&sortBy=timestamp&sortOrder=desc&projectName=react"
```

**Delete a scan:**
```bash
curl -X DELETE http://localhost:5000/api/scan/{scanId}
```

## Usage

### Basic Workflow
1. Start both the backend API and frontend application
2. Navigate to `http://localhost:5173` in your browser
3. Click "Start New Scan" or navigate to the "New Scan" page
4. Upload a ZIP file of your project
5. Optionally provide a project name
6. Click "Scan Project"
7. View the results showing detected languages, frameworks, and tools

### Duplicate Detection
- If you upload a file that was previously scanned, you'll see a dialog
- Choose to view the existing scan or cancel
- Prevents unnecessary re-scanning of identical projects

### Managing Scan History
1. Navigate to "History" page
2. **Filter**: Use the filter panel to search by project name, language, or framework
3. **Sort**: Sort results by date or project name (ascending/descending)
4. **Paginate**: Use pagination controls to browse through results
5. **Delete**: Click the delete icon to remove unwanted scans (with confirmation)

### Deleting Scans
- From **History page**: Click the delete icon next to any scan
- From **Results page**: Click the delete icon in the top-right corner
- Confirm the deletion in the dialog
- The scan will be permanently removed

## Configuration

### Backend (appsettings.json)
```json
{
  "StorageFilePath": "scan-results.json",
  "MaxUploadSizeBytes": 52428800  // 50 MB (configurable: 1KB to 1GB)
}
```

### Frontend (scanService.ts)
```typescript
const API_BASE_URL = 'http://localhost:5000/api';
```

## Data Persistence

Scan results are stored in a JSON file (`scan-results.json`) in the API's root directory. This file persists across server restarts, ensuring scan history is maintained.

**Storage Location:** `CodebaseTechnologyScanner.API/scan-results.json`

## Error Handling

All API errors follow a standardized format:
```json
{
  "error": "Human-readable error message",
  "statusCode": 400,
  "details": "Additional context (optional)"
}
```

### Special Error Cases

| Status Code | Scenario | Additional Fields |
|-------------|----------|-------------------|
| `400` | Bad Request | Standard error format |
| `404` | Scan Not Found | Standard error format |
| `409` | Duplicate Scan | `existingScanId` included |
| `413` | File Too Large | Maximum size in `details` |
| `500` | Server Error | Error details in `details` |

## Technology Stack Summary

### Backend
- **.NET 8** - Modern, high-performance framework
- **ASP.NET Core Web API** - RESTful API framework
- **System.IO.Compression** - ZIP file handling
- **System.Text.Json** - JSON serialization
- **SHA256** - File hash for duplicate detection
- **Swagger/OpenAPI** - API documentation

### Frontend
- **React 19** - Latest React with concurrent features
- **TypeScript** - Type-safe JavaScript
- **Vite** - Fast build tool and dev server
- **Material-UI (MUI)** - Modern React component library
- **React Router v7** - Client-side routing
- **Emotion** - CSS-in-JS styling
- **Axios** - HTTP client (via fetch wrapper)

### Storage
- **File-based JSON storage** - No external database required
- **In-memory caching** - Fast data access
- **Atomic writes** - Data consistency

## Advanced Features

### Duplicate Detection
- Automatic detection of identical project uploads
- SHA256 hash comparison for file integrity
- Option to view existing scan instead of re-scanning
- Saves processing time and storage

### Pagination
- Efficient browsing of large scan histories
- Configurable page size (1-100)
- First/Previous/Next/Last navigation
- Total count and current page display
- URL-based state for bookmarking

### Filtering
- Filter by project name (case-insensitive partial match)
- Filter by detected language
- Filter by detected framework
- Combine multiple filters
- Real-time results update

### Sorting
- Sort by timestamp (newest/oldest first)
- Sort by project name (A-Z or Z-A)
- Persistent across filter changes
- Stable sort algorithm

### Delete Functionality
- Remove unwanted scans from history
- Confirmation dialog prevents accidental deletion
- Automatic data refresh after deletion
- Cascade delete (removes all associated data)

## Detected Technologies

The scanner can detect:

### Languages
- C#, JavaScript, TypeScript, Python, Java, Go, Rust, PHP, Ruby, Swift, Kotlin, and more

### Frameworks & Libraries
- **Frontend**: React, Angular, Vue.js, Svelte, Next.js, Vite
- **Backend**: ASP.NET Core, Express.js, Django, Flask, Spring Boot
- **Mobile**: React Native, Flutter, Xamarin
- **Testing**: Jest, Vitest, xUnit, NUnit, MSTest, Moq

### Build Tools & Package Managers
- npm, Yarn, NuGet, pip, Maven, Gradle, Cargo, Composer

### Configuration Files
- package.json, *.csproj, requirements.txt, pom.xml, build.gradle, Cargo.toml

## Troubleshooting

### Backend Issues

**Problem:** API won't start
- **Solution**: Ensure .NET 8 SDK is installed: `dotnet --version`
- Check if port 5000/7094 is already in use

**Problem:** File upload fails
- **Solution**: Check file size is under the configured limit
- Ensure the file is a valid ZIP archive
- Check disk space availability

**Problem:** Scan results not persisting
- **Solution**: Verify write permissions for `scan-results.json`
- Check storage file path in `appsettings.json`

### Frontend Issues

**Problem:** Can't connect to API
- **Solution**: Verify API is running on `http://localhost:5000`
- Check CORS settings in `Program.cs`
- Verify API URL in `scanService.ts`

**Problem:** Build fails
- **Solution**: Delete `node_modules` and run `npm install` again
- Clear Vite cache: `npm run build -- --force`

**Problem:** Page not found (404) on refresh
- **Solution**: This is expected in dev mode. Use the app's navigation or go to the home page

## Testing

### Backend Testing
```bash
cd CodebaseTechnologyScanner.API
dotnet test
```

### Frontend Testing
```bash
cd codebase-scanner-ui
npm test
```

## Building for Production

### Backend
```bash
cd CodebaseTechnologyScanner.API
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd codebase-scanner-ui
npm run build
```

The production build will be in the `dist/` directory.

## Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- UI powered by [Material-UI](https://mui.com/)
- Frontend built with [Vite](https://vitejs.dev/)
- Icons from [Material Icons](https://fonts.google.com/icons)

---

**Note:** This application is designed for development and educational purposes. 