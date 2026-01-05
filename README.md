# Codebase Technology Scanner

A full-stack web application that scans project folders (uploaded as ZIP files) and detects programming languages, frameworks, tools, and libraries used in the project.

## Features

### Core Functionality
- ? ZIP file upload and scanning
- ? Technology detection (languages, frameworks, tools)
- ? **Duplicate scan detection** - Prevents re-scanning identical projects
- ? **Pagination** - Browse through scan history efficiently
- ? **Filtering** - Filter scans by project name, language, or framework
- ? **Sorting** - Sort by date or project name
- ? **Delete scans** - Remove unwanted scan results
- ? Persistent storage - Data survives server restarts
- ? Configurable file size limits

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
??? CodebaseTechnologyScanner.API/     # Backend .NET Web API
?   ??? Controllers/
?   ?   ??? ScanController.cs
?   ??? Services/
?   ?   ??? IScanService.cs
?   ?   ??? ScanService.cs
?   ?   ??? TechnologyDetector.cs
?   ??? Repositories/
?   ?   ??? IScanRepository.cs
?   ?   ??? FileScanRepository.cs
?   ??? Models/
?   ?   ??? ScanRequest.cs
?   ?   ??? ScanResult.cs
?   ?   ??? TechnologyInfo.cs
?   ?   ??? ErrorResponse.cs
?   ?   ??? PaginatedResponse.cs
?   ?   ??? DeleteResponse.cs
?   ??? Program.cs
?   ??? appsettings.json
?
??? codebase-scanner-ui/               # Frontend React App
    ??? src/
    ?   ??? components/
    ?   ?   ??? Header.tsx
    ?   ?   ??? Footer.tsx
    ?   ?   ??? ScanResultCard.tsx
    ?   ??? layouts/
    ?   ?   ??? MainLayout.tsx
    ?   ??? pages/
    ?   ?   ??? HomePage.tsx
    ?   ?   ??? NewScanPage.tsx
    ?   ?   ??? ScanResultsPage.tsx
    ?   ?   ??? ScanHistoryPage.tsx
    ?   ??? services/
    ?   ?   ??? scanService.ts
    ?   ??? models/
    ?   ?   ??? ScanRequest.ts
    ?   ?   ??? ScanResult.ts
    ?   ?   ??? TechnologyInfo.ts
    ?   ?   ??? ErrorResponse.ts
    ?   ?   ??? PaginatedResponse.ts
    ?   ?   ??? DeleteResponse.ts
    ?   ??? App.tsx
    ?   ??? main.tsx
    ??? package.json
```

## Pages

1. **Home Page** (`/`) - Landing page with application overview
2. **New Scan Page** (`/scan`) - Upload ZIP file and initiate scan
3. **Scan Results Page** (`/results/:scanId`) - Detailed scan results with delete option
4. **Scan History Page** (`/history`) - Paginated list with filtering, sorting, and delete

## API Endpoints

### Upload and Scan
- **POST** `/api/scan/upload` - Upload ZIP file and perform scan
  - **Request**: `multipart/form-data` with `file` and optional `projectName`
  - **Response**: Scan result object
  - **Error Codes**: 400 (invalid file), 409 (duplicate), 413 (file too large), 500 (processing error)

### Get Scan Result
- **GET** `/api/scan/{id}` - Get specific scan result
  - **Response**: Scan result object
  - **Error Codes**: 404 (not found), 500 (server error)

### Get Scan History
- **GET** `/api/scan/history` - Get paginated scan results with filtering
  - **Query Parameters**:
    - `page` (default: 1)
    - `pageSize` (default: 20, max: 100)
    - `sortBy` (timestamp | projectName)
    - `sortOrder` (asc | desc)
    - `projectName` (filter)
    - `language` (filter)
    - `framework` (filter)
  - **Response**: Paginated response with results and metadata
  - **Error Codes**: 400 (invalid parameters), 500 (server error)

### Delete Scan
- **DELETE** `/api/scan/{id}` - Delete scan result
  - **Response**: Delete confirmation with deleted ID
  - **Error Codes**: 404 (not found), 500 (server error)

## Technology Detection Capabilities

### Languages Detected
C#, JavaScript, TypeScript, Python, Java, C++, C, Go, Rust, Ruby, PHP, Swift, Kotlin, Scala, R, Objective-C, SQL, Shell, PowerShell, Dart, Lua, Perl, Groovy, and more

### Frameworks & Tools Detected
- **JavaScript/TypeScript**: React, Vue.js, Angular, Next.js, Nuxt.js, Svelte, Express.js, NestJS, Gatsby
- **.NET**: .NET Framework versions from project files
- **Package Managers**: npm, Yarn, pnpm, pip, Pipenv, Poetry, Bundler, Cargo, Go Modules, Composer
- **Build Tools**: Gradle, Maven, Webpack, Vite
- **Containers**: Docker, Docker Compose
- **Testing**: Jest, Karma, pytest
- **Linters/Formatters**: ESLint, Prettier
- **UI Libraries**: Material-UI, Ant Design, Bootstrap, Tailwind CSS
- **Other Libraries**: Redux, Axios, Lodash

## Setup and Running

### Backend

1. Navigate to the API directory:
   ```bash
   cd CodebaseTechnologyScanner.API
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Run the API:
   ```bash
   dotnet run
   ```

   The API will start on `http://localhost:5000` (or `https://localhost:5001`)

### Frontend

1. Navigate to the UI directory:
   ```bash
   cd codebase-scanner-ui
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Update API URL if needed in `src/services/scanService.ts`

4. Run the development server:
   ```bash
   npm run dev
   ```

   The app will start on `http://localhost:5173`

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
  "MaxUploadSizeBytes": 52428800  // 50 MB (configurable)
}
```

### Frontend (scanService.ts)
```typescript
const API_BASE_URL = 'http://localhost:5000/api';
```

## Data Persistence

Scan results are stored in a JSON file (`scan-results.json`) in the API's root directory. This file persists across server restarts, ensuring scan history is maintained.

## Error Handling

All API errors follow a standardized format:
```json
{
  "error": "Human-readable error message",
  "statusCode": 400,
  "details": "Additional context (optional)"
}
```

**Special Cases:**
- **409 Duplicate**: Includes `existingScanId` to navigate to existing scan
- **413 File Too Large**: Includes maximum allowed size in details

## Notes

- **Maximum file upload size**: Configurable in `appsettings.json` (default: 50MB)
- **Only ZIP files** are accepted
- **Duplicate detection**: Uses SHA256 file hash
- **Version detection**: Relies on configuration files where available
- **Pagination**: Default page size is 20 (max 100)

## Technology Stack Summary

**Backend:**
- .NET 8
- ASP.NET Core Web API
- System.IO.Compression for ZIP handling
- System.Text.Json for serialization
- SHA256 for duplicate detection

**Frontend:**
- React 19
- TypeScript
- Vite
- Material-UI (MUI)
- React Router v7
- Emotion (CSS-in-JS)

**Storage:**
- File-based JSON storage (no external database required)

## Advanced Features

### Duplicate Detection
- Automatic detection of identical project uploads
- SHA256 hash comparison
- Option to view existing scan instead of re-scanning

### Pagination
- Efficient browsing of large scan histories
- Configurable page size
- First/Previous/Next/Last navigation
- Result count display

### Filtering
- Filter by project name (case-insensitive partial match)
- Filter by detected language
- Filter by detected framework
- Combine multiple filters

### Sorting
- Sort by timestamp (newest/oldest first)
- Sort by project name (A-Z or Z-A)
- Persistent across filter changes

### Delete Functionality
- Remove unwanted scans from history
- Confirmation dialog prevents accidental deletion
- Automatic data refresh after deletion

## Future Enhancements

- Authentication and user management
- Cloud storage integration
- More sophisticated version detection
- Dependency tree visualization
- Security vulnerability scanning
- Export reports to PDF/CSV
- Real-time scanning progress updates
- Batch upload and scanning
