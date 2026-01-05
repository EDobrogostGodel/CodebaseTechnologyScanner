using CodebaseTechnologyScanner.API.Repositories;
using CodebaseTechnologyScanner.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Codebase Technology Scanner API",
        Version = "v1",
        Description = "API for scanning project ZIP files and detecting technologies, languages, and frameworks"
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddSingleton<IScanRepository, FileScanRepository>();
builder.Services.AddScoped<TechnologyDetector>();
builder.Services.AddScoped<IScanService, ScanService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Validate configuration on startup
var config = app.Configuration;
var maxSize = config.GetValue<long>("MaxUploadSizeBytes", 52428800);
if (maxSize < 1024 || maxSize > 1073741824) // 1KB to 1GB
{
    throw new InvalidOperationException(
        $"MaxUploadSizeBytes must be between 1KB (1024 bytes) and 1GB (1073741824 bytes). Current value: {maxSize} bytes");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
