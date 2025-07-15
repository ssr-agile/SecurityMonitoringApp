using Gelf.Extensions.Logging;
using SecurityMonitoringApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddHttpClient<GraylogService>();
builder.Services.AddHttpClient<WazuhService>();

// Register services
builder.Services.AddScoped<GraylogService>();
builder.Services.AddScoped<WazuhService>();

builder.Services.AddControllers();

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Graylog logging only if Graylog host is configured
var graylogHost = builder.Configuration["Graylog:Host"];
var graylogPort = builder.Configuration.GetValue<int>("Graylog:Port");

if (!string.IsNullOrEmpty(graylogHost) && graylogPort > 0)
{
    builder.Logging.AddGelf(options => {
        options.Host = graylogHost;
        options.Port = graylogPort;
        options.LogSource = "SecurityMonitoringApp";
    });
}

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();

// basic endpoints for testing
app.MapGet("/", () => {
    return Results.Ok(new
    {
        Message = "Security Monitoring App is Running!",
        Timestamp = DateTime.UtcNow,
        Environment = app.Environment.EnvironmentName
    });
});

app.MapGet("/health", () => {
    return Results.Ok(new
    {
        Status = "Healthy",
        Timestamp = DateTime.UtcNow,
        Version = "1.0.0"
    });
});

app.MapControllers();

// Add better startup logging
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting up...");
logger.LogInformation("Environment: {Environment}", app.Environment.EnvironmentName);
logger.LogInformation("Graylog Host: {Host}", graylogHost);

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application failed to start");
    throw;
}