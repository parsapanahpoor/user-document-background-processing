using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using UserDocumentAPI.BackgroundJobs;
using UserDocumentAPI.Data;
using UserDocumentAPI.HealthChecks;
using UserDocumentAPI.Services;

// تنظیمات Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Hangfire", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting User Document API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Database
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            }));

    // Hangfire
    builder.Services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            builder.Configuration.GetConnectionString("HangfireConnection"),
            new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

    builder.Services.AddHangfireServer(options =>
    {
        options.WorkerCount = 5;
    });

    // Services
    builder.Services.Configure<FileStorageOptions>(
        builder.Configuration.GetSection("FileStorage"));
    builder.Services.AddScoped<IFileService, FileService>();

    // Background Jobs Configuration
    builder.Services.Configure<CleanupOptions>(
        builder.Configuration.GetSection("Cleanup"));
    
    // Background Jobs
    builder.Services.AddScoped<WelcomeMessageJob>();
    builder.Services.AddScoped<DocumentProcessingJob>();
    builder.Services.AddScoped<CompletionMessageJob>();
    builder.Services.AddScoped<NightlyCleanupJob>();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<HangfireHealthCheck>("hangfire")
        .AddCheck<StorageHealthCheck>("storage");

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "User Document API",
            Version = "v1",
            Description = "Background job processing for user document management with Hangfire",
            Contact = new OpenApiContact
            {
                Name = "Your Name",
                Email = "your.email@example.com",
                Url = new Uri("https://github.com/your-username")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // XML Comments برای Swagger Documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    // Middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "User Document API v1");
            c.RoutePrefix = string.Empty; // ✅ Swagger در root باز شود
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    // Health Check Endpoint
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    data = e.Value.Data
                }),
                timestamp = DateTime.UtcNow
            });
            await context.Response.WriteAsync(result);
        }
    });

    // Hangfire Dashboard
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter(allowAll: app.Environment.IsDevelopment()) },
        DashboardTitle = "User Document API - Jobs Dashboard"
    });

    // Recurring Job - اجرا هر شب ساعت 00:00
    RecurringJob.AddOrUpdate<NightlyCleanupJob>(
        "nightly-cleanup",
        job => job.CleanupAsync(),
        "0 0 * * *",
        new RecurringJobOptions
        {
            TimeZone = TimeZoneInfo.Local
        });

    // Global Retry Policy
    GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
    {
        Attempts = 2,
        DelaysInSeconds = new[] { 300, 600 }
    });

    // Database Migration
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Log.Information("Database migration completed");
    }

    Log.Information("Application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    private readonly bool _allowAll;

    public HangfireAuthorizationFilter(bool allowAll = false)
    {
        _allowAll = allowAll;
    }

    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // در Development همه دسترسی دارند
        if (_allowAll)
            return true;

        var httpContext = context.GetHttpContext();

        // در Production فقط از localhost دسترسی مجاز است
        // یا می‌تونید از Authentication/Authorization استفاده کنید
        return httpContext.Connection.RemoteIpAddress?.ToString() == "127.0.0.1" 
            || httpContext.Connection.RemoteIpAddress?.ToString() == "::1"
            || httpContext.Connection.LocalIpAddress?.Equals(httpContext.Connection.RemoteIpAddress) == true;
    }
}
