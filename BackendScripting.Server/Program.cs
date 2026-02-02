using Serilog;
using BackendScript.Persistence;
using BackendScripting.Scripting;
using BackendScripting.Server.Endpoints;
using BackendScripting.Server.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("log.txt")
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Application started.");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// ioc
builder.Services.AddSingleton<DbContext>();
builder.Services.AddScoped<IStockQuoteRepository, StockQuoteRepository>();
builder.Services.AddScoped<IStockQuoteScriptRepository, StockQuoteScriptRepository>();
builder.Services.AddScoped<IStockQuoteResultRepository, StockQuoteResultRepository>();
builder.Services.AddScoped<IStockQuoteMockService, StockQuoteMockService>();
builder.Services.AddScoped<IStockQuoteEvaluationService, StockQuoteEvaluationService>();

// build app
var app = builder.Build();

// dev
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//pp.UseHttpsRedirection();

// endpoints
app.MapStockQuoteEndpoints();
app.MapStockQuoteScriptEndpoints();
app.MapStockQuoteResultEndpoints();

// complier
var initCompilerConfig = builder.Configuration["InitializeScriptCompiler"];
if (bool.TryParse(initCompilerConfig, out var initComplier) && initComplier)
{
    CompilerBootstrap.Initialize();
}

// scripting assemblies
var timeoutConfig = builder.Configuration["AssemblyCacheTimeout"];
if (!TimeSpan.TryParse(timeoutConfig, out var timeout))
{
    timeout = TimeSpan.FromMinutes(30);
}
AssemblyFactory.SetCacheTimeout(timeout);

app.Run();

Log.Information("Application ended.");
