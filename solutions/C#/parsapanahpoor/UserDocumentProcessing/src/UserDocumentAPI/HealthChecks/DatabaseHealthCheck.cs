using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserDocumentAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace UserDocumentAPI.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(AppDbContext context, ILogger<DatabaseHealthCheck> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // تست اتصال به دیتابیس
            await _context.Database.CanConnectAsync(cancellationToken);
            
            // تست کوئری ساده
            var userCount = await _context.Users.CountAsync(cancellationToken);

            var data = new Dictionary<string, object>
            {
                { "user_count", userCount },
                { "connection_state", _context.Database.GetConnectionString() != null ? "Connected" : "Disconnected" }
            };

            return HealthCheckResult.Healthy("Database is healthy", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
        }
    }
}
