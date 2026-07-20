using CafeSphere.API.Hubs;
using CafeSphere.API.Middleware;
using CafeSphere.API.Services;
using CafeSphere.Application;
using CafeSphere.Application.Interfaces;
using CafeSphere.Infrastructure;
using CafeSphere.Persistence;
using CafeSphere.Persistence.Context;
using CafeSphere.Persistence.Seed;
using Microsoft.OpenApi;
using Serilog;

// Load Environment variables from .env file if present
var currentDir = Directory.GetCurrentDirectory();
var envPath = Path.Combine(currentDir, ".env");
if (!File.Exists(envPath))
{
    var parentEnvPath = Path.Combine(currentDir, "..", "..", ".env");
    if (File.Exists(parentEnvPath))
    {
        DotNetEnv.Env.Load(parentEnvPath);
    }
}
else
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);

// Add Environment Variables to Configuration Provider
builder.Configuration.AddEnvironmentVariables();

// Configure Serilog Structured Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/cafesphere-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Register Clean Architecture Layer Services
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Register API Services
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// Configure CORS reading from .env
var corsAllowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) 
    ?? new[] { "http://localhost:5288", "http://localhost:5000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure Swagger with JWT Bearer Authentication
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CafeSphere Backend API",
        Version = "v1",
        Description = "Enterprise Production-Ready Cafe Management Platform Web API"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token format: 'Bearer {your_token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Seed MongoDB Indexes & System Roles on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var mongoContext = scope.ServiceProvider.GetRequiredService<IMongoDbContext>();
        await MongoDbInitializer.InitializeAsync(mongoContext);
        Log.Information("MongoDB Atlas initialization & index creation completed.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing MongoDB Atlas database.");
    }
}

// Global Exception Handler Middleware (RFC 7807)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CafeSphere API v1");
    });
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub Endpoints
app.MapHub<KitchenHub>("/hubs/kitchen");
app.MapHub<PosHub>("/hubs/pos");
app.MapHub<DashboardHub>("/hubs/dashboard");
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
