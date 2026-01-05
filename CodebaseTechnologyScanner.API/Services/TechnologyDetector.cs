using System.IO.Compression;
using CodebaseTechnologyScanner.API.Models;

namespace CodebaseTechnologyScanner.API.Services;

public class TechnologyDetector
{
    private readonly ILogger<TechnologyDetector> _logger;

    private readonly Dictionary<string, string> _fileExtensionToLanguage = new()
    {
        { ".cs", "C#" },
        { ".csproj", "C#" },
        { ".vb", "Visual Basic" },
        { ".fs", "F#" },
        { ".js", "JavaScript" },
        { ".jsx", "JavaScript" },
        { ".ts", "TypeScript" },
        { ".tsx", "TypeScript" },
        { ".py", "Python" },
        { ".java", "Java" },
        { ".cpp", "C++" },
        { ".c", "C" },
        { ".h", "C/C++" },
        { ".hpp", "C++" },
        { ".go", "Go" },
        { ".rs", "Rust" },
        { ".rb", "Ruby" },
        { ".php", "PHP" },
        { ".swift", "Swift" },
        { ".kt", "Kotlin" },
        { ".scala", "Scala" },
        { ".r", "R" },
        { ".m", "Objective-C" },
        { ".sql", "SQL" },
        { ".sh", "Shell" },
        { ".ps1", "PowerShell" },
        { ".dart", "Dart" },
        { ".lua", "Lua" },
        { ".pl", "Perl" },
        { ".groovy", "Groovy" }
    };

    private readonly Dictionary<string, (string Type, string Name)> _configFileDetection = new()
    {
        { "package.json", ("Package Manager", "npm") },
        { "package-lock.json", ("Package Manager", "npm") },
        { "yarn.lock", ("Package Manager", "Yarn") },
        { "pnpm-lock.yaml", ("Package Manager", "pnpm") },
        { "requirements.txt", ("Package Manager", "pip") },
        { "Pipfile", ("Package Manager", "Pipenv") },
        { "poetry.lock", ("Package Manager", "Poetry") },
        { "Gemfile", ("Package Manager", "Bundler") },
        { "Cargo.toml", ("Package Manager", "Cargo") },
        { "go.mod", ("Package Manager", "Go Modules") },
        { "composer.json", ("Package Manager", "Composer") },
        { "build.gradle", ("Build Tool", "Gradle") },
        { "pom.xml", ("Build Tool", "Maven") },
        { ".csproj", ("Project File", ".NET") },
        { ".sln", ("Project File", ".NET Solution") },
        { "Dockerfile", ("Container", "Docker") },
        { "docker-compose.yml", ("Container", "Docker Compose") },
        { ".gitignore", ("Version Control", "Git") },
        { "tsconfig.json", ("Configuration", "TypeScript") },
        { "webpack.config.js", ("Build Tool", "Webpack") },
        { "vite.config.js", ("Build Tool", "Vite") },
        { "vite.config.ts", ("Build Tool", "Vite") },
        { "angular.json", ("Framework", "Angular") },
        { "nuxt.config.js", ("Framework", "Nuxt.js") },
        { "next.config.js", ("Framework", "Next.js") },
        { "vue.config.js", ("Framework", "Vue.js") },
        { ".eslintrc", ("Linter", "ESLint") },
        { ".eslintrc.json", ("Linter", "ESLint") },
        { ".prettierrc", ("Formatter", "Prettier") },
        { "jest.config.js", ("Testing", "Jest") },
        { "karma.conf.js", ("Testing", "Karma") },
        { "pytest.ini", ("Testing", "pytest") }
    };

    public TechnologyDetector(ILogger<TechnologyDetector> logger)
    {
        _logger = logger;
    }

    public async Task<List<TechnologyInfo>> DetectTechnologiesAsync(string extractPath)
    {
        var technologies = new List<TechnologyInfo>();
        var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var extension = Path.GetExtension(file).ToLowerInvariant();

            if (_configFileDetection.ContainsKey(fileName))
            {
                var (type, name) = _configFileDetection[fileName];
                var tech = new TechnologyInfo
                {
                    Name = name,
                    Type = type,
                    DetectedFrom = fileName
                };

                if (fileName == "package.json")
                {
                    await DetectPackageJsonDetailsAsync(file, technologies);
                }
                else if (extension == ".csproj")
                {
                    await DetectCsprojDetailsAsync(file, technologies);
                }

                if (!technologies.Any(t => t.Name == name && t.Type == type))
                {
                    technologies.Add(tech);
                }
            }
        }

        return technologies.DistinctBy(t => $"{t.Type}-{t.Name}").ToList();
    }

    public Dictionary<string, int> AnalyzeLanguages(string extractPath)
    {
        var languageCounts = new Dictionary<string, int>();
        var files = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file).ToLowerInvariant();
            if (_fileExtensionToLanguage.TryGetValue(extension, out var language))
            {
                languageCounts[language] = languageCounts.GetValueOrDefault(language, 0) + 1;
            }
        }

        return languageCounts;
    }

    private async Task DetectPackageJsonDetailsAsync(string filePath, List<TechnologyInfo> technologies)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var doc = System.Text.Json.JsonDocument.Parse(json);
            
            if (doc.RootElement.TryGetProperty("dependencies", out var deps))
            {
                DetectFrameworksFromDependencies(deps, technologies);
            }

            if (doc.RootElement.TryGetProperty("devDependencies", out var devDeps))
            {
                DetectFrameworksFromDependencies(devDeps, technologies);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse package.json at {FilePath}", filePath);
        }
    }

    private void DetectFrameworksFromDependencies(System.Text.Json.JsonElement dependencies, List<TechnologyInfo> technologies)
    {
        var frameworkDetectors = new Dictionary<string, (string Type, string Name)>
        {
            { "react", ("Framework", "React") },
            { "vue", ("Framework", "Vue.js") },
            { "@angular/core", ("Framework", "Angular") },
            { "next", ("Framework", "Next.js") },
            { "nuxt", ("Framework", "Nuxt.js") },
            { "svelte", ("Framework", "Svelte") },
            { "express", ("Framework", "Express.js") },
            { "nestjs", ("Framework", "NestJS") },
            { "gatsby", ("Framework", "Gatsby") },
            { "redux", ("Library", "Redux") },
            { "axios", ("Library", "Axios") },
            { "lodash", ("Library", "Lodash") },
            { "tailwindcss", ("CSS Framework", "Tailwind CSS") },
            { "bootstrap", ("CSS Framework", "Bootstrap") },
            { "@mui/material", ("UI Library", "Material-UI") },
            { "antd", ("UI Library", "Ant Design") }
        };

        foreach (var prop in dependencies.EnumerateObject())
        {
            var packageName = prop.Name;
            var version = prop.Value.GetString()?.TrimStart('^', '~') ?? "";

            if (frameworkDetectors.TryGetValue(packageName, out var framework))
            {
                technologies.Add(new TechnologyInfo
                {
                    Name = framework.Name,
                    Type = framework.Type,
                    Version = version,
                    DetectedFrom = "package.json"
                });
            }
        }
    }

    private async Task DetectCsprojDetailsAsync(string filePath, List<TechnologyInfo> technologies)
    {
        try
        {
            var xml = await File.ReadAllTextAsync(filePath);
            
            if (xml.Contains("<TargetFramework>"))
            {
                var start = xml.IndexOf("<TargetFramework>") + "<TargetFramework>".Length;
                var end = xml.IndexOf("</TargetFramework>");
                if (end > start)
                {
                    var framework = xml.Substring(start, end - start);
                    technologies.Add(new TechnologyInfo
                    {
                        Name = ".NET",
                        Type = "Framework",
                        Version = framework,
                        DetectedFrom = Path.GetFileName(filePath)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse .csproj at {FilePath}", filePath);
        }
    }
}
