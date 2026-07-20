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

// Configure CORS reading directly from .env
var corsAllowedOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"]?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) 
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(corsAllowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
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
        Log.Information("MongoDB initialization & index creation completed successfully.");
    }
    catch (MongoDB.Driver.MongoAuthenticationException authEx)
    {
        Log.Warning("MongoDB Atlas Authentication Failed: {Message}. Please verify your database credentials in backend/.env", authEx.Message);
    }
    catch (TimeoutException timeEx)
    {
        Log.Warning("MongoDB database is not reachable ({Message}). API will continue running without local MongoDB.", timeEx.Message);
    }
    catch (MongoDB.Driver.MongoConnectionException connEx)
    {
        Log.Warning("MongoDB database connection refused ({Message}). API will continue running without local MongoDB.", connEx.Message);
    }
    catch (Exception ex)
    {
        Log.Warning("Skipping MongoDB startup initialization: {Message}", ex.Message);
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
