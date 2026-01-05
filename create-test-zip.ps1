# Generate Single Test ZIP File
# Creates a realistic project ZIP for testing the Codebase Technology Scanner

param(
    [string]$OutputPath = ".\test-upload.zip",
    [switch]$Complex
)

Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Cyan
Write-Host "?  Single Test ZIP Generator                                 ?" -ForegroundColor Cyan
Write-Host "??????????????????????????????????????????????????????????????`n" -ForegroundColor Cyan

# Clean up existing file
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Force
    Write-Host "?? Removed existing file: $OutputPath`n" -ForegroundColor Yellow
}

# Create temporary directory
$tempDir = Join-Path $env:TEMP "test-project-$(Get-Random)"
New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

Write-Host "?? Creating test project structure..." -ForegroundColor Cyan

# ============================================================
# Create Project Files
# ============================================================

if ($Complex) {
    Write-Host "   ?? Complex mode: Creating full-stack project...`n" -ForegroundColor Yellow
    
    # Backend folder
    $backendDir = Join-Path $tempDir "backend"
    New-Item -Path $backendDir -ItemType Directory -Force | Out-Null
    
    # .NET Project file
    @"
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="8.0.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
  </ItemGroup>
</Project>
"@ | Out-File -FilePath (Join-Path $backendDir "TestAPI.csproj") -Encoding UTF8

    # Program.cs
    @"
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
"@ | Out-File -FilePath (Join-Path $backendDir "Program.cs") -Encoding UTF8

    # Controller
    @"
using Microsoft.AspNetCore.Mvc;

namespace TestAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(new[] { "Product 1", "Product 2" });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        return Ok(new { Id = id, Name = "Product" });
    }

    [HttpPost]
    public IActionResult Create([FromBody] object product)
    {
        return Created("", product);
    }
}
"@ | Out-File -FilePath (Join-Path $backendDir "ProductsController.cs") -Encoding UTF8

    # Service
    @"
namespace TestAPI.Services;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
}

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }
}
"@ | Out-File -FilePath (Join-Path $backendDir "ProductService.cs") -Encoding UTF8

    # Frontend folder
    $frontendDir = Join-Path $tempDir "frontend"
    New-Item -Path $frontendDir -ItemType Directory -Force | Out-Null
    
    # package.json
    @"
{
  "name": "test-react-app",
  "version": "1.0.0",
  "description": "Test React Application",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "vitest"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.20.0",
    "@mui/material": "^5.14.0",
    "@mui/icons-material": "^5.14.0",
    "@emotion/react": "^11.11.0",
    "@emotion/styled": "^11.11.0",
    "axios": "^1.6.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "@vitejs/plugin-react": "^4.2.0",
    "typescript": "^5.2.0",
    "vite": "^5.0.0",
    "vitest": "^1.0.0",
    "@testing-library/react": "^14.1.0"
  }
}
"@ | Out-File -FilePath (Join-Path $frontendDir "package.json") -Encoding UTF8

    # App.tsx
    @"
import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AppBar, Toolbar, Typography, Container, Box } from '@mui/material';
import HomePage from './pages/HomePage';
import ProductsPage from './pages/ProductsPage';

const App: React.FC = () => {
  const [products, setProducts] = useState<any[]>([]);

  useEffect(() => {
    fetchProducts();
  }, []);

  const fetchProducts = async () => {
    try {
      const response = await fetch('/api/products');
      const data = await response.json();
      setProducts(data);
    } catch (error) {
      console.error('Error fetching products:', error);
    }
  };

  return (
    <BrowserRouter>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6">Test Application</Typography>
        </Toolbar>
      </AppBar>
      <Container>
        <Box sx={{ mt: 4 }}>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/products" element={<ProductsPage products={products} />} />
          </Routes>
        </Box>
      </Container>
    </BrowserRouter>
  );
};

export default App;
"@ | Out-File -FilePath (Join-Path $frontendDir "App.tsx") -Encoding UTF8

    # Component
    @"
import React from 'react';
import { Card, CardContent, Typography, Button } from '@mui/material';

interface ProductCardProps {
  product: {
    id: number;
    name: string;
    price: number;
  };
  onView: (id: number) => void;
}

export const ProductCard: React.FC<ProductCardProps> = ({ product, onView }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h5">{product.name}</Typography>
        <Typography variant="body2" color="text.secondary">
          Price: `${product.price}
        </Typography>
        <Button onClick={() => onView(product.id)}>View Details</Button>
      </CardContent>
    </Card>
  );
};
"@ | Out-File -FilePath (Join-Path $frontendDir "ProductCard.tsx") -Encoding UTF8

    # vite.config.ts
    @"
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
});
"@ | Out-File -FilePath (Join-Path $frontendDir "vite.config.ts") -Encoding UTF8

    # tsconfig.json
    @"
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
"@ | Out-File -FilePath (Join-Path $frontendDir "tsconfig.json") -Encoding UTF8

    # Dockerfile
    @"
FROM node:18-alpine AS frontend-build
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm install
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /app/backend
COPY backend/*.csproj ./
RUN dotnet restore
COPY backend/ ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=backend-build /app/backend/out .
COPY --from=frontend-build /app/frontend/dist ./wwwroot
ENTRYPOINT ["dotnet", "TestAPI.dll"]
"@ | Out-File -FilePath (Join-Path $tempDir "Dockerfile") -Encoding UTF8

    # docker-compose.yml
    @"
version: '3.8'
services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    depends_on:
      - db
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
"@ | Out-File -FilePath (Join-Path $tempDir "docker-compose.yml") -Encoding UTF8

    # README.md
    @"
# Test Full-Stack Application

A full-stack application with .NET 8 backend and React frontend.

## Technologies

### Backend
- .NET 8 Web API
- Entity Framework Core
- Swagger/OpenAPI

### Frontend
- React 18
- TypeScript
- Material-UI
- Vite
- React Router

### DevOps
- Docker
- Docker Compose

## Getting Started

### Backend
\`\`\`bash
cd backend
dotnet restore
dotnet run
\`\`\`

### Frontend
\`\`\`bash
cd frontend
npm install
npm run dev
\`\`\`

### Docker
\`\`\`bash
docker-compose up
\`\`\`
"@ | Out-File -FilePath (Join-Path $tempDir "README.md") -Encoding UTF8

    # .gitignore
    @"
# .NET
bin/
obj/
*.user
*.suo

# Node
node_modules/
dist/
.env.local

# IDE
.vscode/
.idea/
*.swp
"@ | Out-File -FilePath (Join-Path $tempDir ".gitignore") -Encoding UTF8

} else {
    Write-Host "   ? Simple mode: Creating React + TypeScript project...`n" -ForegroundColor Yellow
    
    # package.json
    @"
{
  "name": "my-react-app",
  "version": "1.0.0",
  "description": "React TypeScript Application",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview"
  },
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "@mui/material": "^5.14.0",
    "axios": "^1.6.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "@vitejs/plugin-react-swc": "^3.5.0",
    "typescript": "^5.2.0",
    "vite": "^5.0.0"
  }
}
"@ | Out-File -FilePath (Join-Path $tempDir "package.json") -Encoding UTF8

    # App.tsx
    @"
import React, { useState } from 'react';
import { Button, Container, Typography, Box } from '@mui/material';

const App: React.FC = () => {
  const [count, setCount] = useState(0);

  return (
    <Container maxWidth="md">
      <Box sx={{ mt: 4, textAlign: 'center' }}>
        <Typography variant="h2" gutterBottom>
          Hello React!
        </Typography>
        <Typography variant="h4" sx={{ mb: 2 }}>
          Count: {count}
        </Typography>
        <Button 
          variant="contained" 
          onClick={() => setCount(count + 1)}
          size="large"
        >
          Increment
        </Button>
      </Box>
    </Container>
  );
};

export default App;
"@ | Out-File -FilePath (Join-Path $tempDir "App.tsx") -Encoding UTF8

    # index.tsx
    @"
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
"@ | Out-File -FilePath (Join-Path $tempDir "index.tsx") -Encoding UTF8

    # Component.tsx
    @"
import React from 'react';
import { Card, CardContent, Typography } from '@mui/material';

interface GreetingProps {
  name: string;
}

export const Greeting: React.FC<GreetingProps> = ({ name }) => {
  return (
    <Card>
      <CardContent>
        <Typography variant="h5">
          Hello, {name}!
        </Typography>
      </CardContent>
    </Card>
  );
};
"@ | Out-File -FilePath (Join-Path $tempDir "Greeting.tsx") -Encoding UTF8

    # vite.config.ts
    @"
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
  },
});
"@ | Out-File -FilePath (Join-Path $tempDir "vite.config.ts") -Encoding UTF8

    # tsconfig.json
    @"
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true
  },
  "include": ["src"]
}
"@ | Out-File -FilePath (Join-Path $tempDir "tsconfig.json") -Encoding UTF8

    # index.css
    @"
body {
  margin: 0;
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Roboto', 'Oxygen',
    'Ubuntu', 'Cantarell', 'Fira Sans', 'Droid Sans', 'Helvetica Neue',
    sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

code {
  font-family: source-code-pro, Menlo, Monaco, Consolas, 'Courier New',
    monospace;
}
"@ | Out-File -FilePath (Join-Path $tempDir "index.css") -Encoding UTF8

    # README.md
    @"
# React TypeScript Application

A simple React application with TypeScript and Material-UI.

## Technologies

- React 18
- TypeScript
- Material-UI
- Vite

## Getting Started

\`\`\`bash
npm install
npm run dev
\`\`\`

## Build

\`\`\`bash
npm run build
\`\`\`
"@ | Out-File -FilePath (Join-Path $tempDir "README.md") -Encoding UTF8
}

# ============================================================
# Create ZIP File
# ============================================================

Write-Host "?? Creating ZIP file..." -ForegroundColor Cyan
Compress-Archive -Path "$tempDir\*" -DestinationPath $OutputPath -Force

# Cleanup
Remove-Item -Path $tempDir -Recurse -Force

# ============================================================
# Summary
# ============================================================

$zipInfo = Get-Item $OutputPath
$sizeKB = "{0:N2} KB" -f ($zipInfo.Length / 1KB)

Write-Host "`n??????????????????????????????????????????????????????????????" -ForegroundColor Green
Write-Host "?  ? ZIP File Created Successfully                          ?" -ForegroundColor Green
Write-Host "??????????????????????????????????????????????????????????????`n" -ForegroundColor Green

Write-Host "?? File: " -ForegroundColor White -NoNewline
Write-Host "$OutputPath" -ForegroundColor Cyan

Write-Host "?? Size: " -ForegroundColor White -NoNewline
Write-Host "$sizeKB" -ForegroundColor Cyan

Write-Host "`n?? Expected Detections:" -ForegroundColor Yellow

if ($Complex) {
    Write-Host "   Languages:" -ForegroundColor White
    Write-Host "   • C# (3 files)" -ForegroundColor Gray
    Write-Host "   • TypeScript (4 files)" -ForegroundColor Gray
    Write-Host "`n   Technologies:" -ForegroundColor White
    Write-Host "   • Framework: .NET (net8.0)" -ForegroundColor Gray
    Write-Host "   • Framework: React" -ForegroundColor Gray
    Write-Host "   • Build Tool: Vite" -ForegroundColor Gray
    Write-Host "   • Package Manager: npm" -ForegroundColor Gray
    Write-Host "   • UI Library: Material-UI" -ForegroundColor Gray
    Write-Host "   • Container: Docker" -ForegroundColor Gray
    Write-Host "   • Container: Docker Compose" -ForegroundColor Gray
    Write-Host "   • Project File: .NET" -ForegroundColor Gray
} else {
    Write-Host "   Languages:" -ForegroundColor White
    Write-Host "   • TypeScript (4 files)" -ForegroundColor Gray
    Write-Host "`n   Technologies:" -ForegroundColor White
    Write-Host "   • Framework: React" -ForegroundColor Gray
    Write-Host "   • Build Tool: Vite" -ForegroundColor Gray
    Write-Host "   • Package Manager: npm" -ForegroundColor Gray
    Write-Host "   • UI Library: Material-UI" -ForegroundColor Gray
}

Write-Host "`n?? Next Steps:" -ForegroundColor Yellow
Write-Host "   1. Start the application: " -ForegroundColor White -NoNewline
Write-Host ".\start.ps1" -ForegroundColor Cyan
Write-Host "   2. Navigate to: " -ForegroundColor White -NoNewline
Write-Host "http://localhost:5173" -ForegroundColor Cyan
Write-Host "   3. Upload file: " -ForegroundColor White -NoNewline
Write-Host "$OutputPath" -ForegroundColor Cyan
Write-Host "   4. Verify detection results`n" -ForegroundColor White
