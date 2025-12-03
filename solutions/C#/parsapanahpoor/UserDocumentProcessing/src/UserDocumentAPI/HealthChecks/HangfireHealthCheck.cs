using Microsoft.Extensions.Diagnostics.HealthChecks;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace UserDocumentAPI.HealthChecks;

public class HangfireHealthCheck : IHealthCheck
{
    private readonly ILogger<HangfireHealthCheck> _logger;

    public HangfireHealthCheck(ILogger<HangfireHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (JobStorage.Current == null)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy("Hangfire JobStorage is not initialized")
                );
            }

            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var statistics = monitoringApi.GetStatistics();

            var data = new Dictionary<string, object>
            {
                { "servers", statistics.Servers },
                { "queues", statistics.Queues },
                { "enqueued", statistics.Enqueued },
                { "processing", statistics.Processing },
                { "scheduled", statistics.Scheduled },
                { "failed", statistics.Failed },
                { "succeeded", statistics.Succeeded },
                { "deleted", statistics.Deleted },
                { "recurring", statistics.Recurring }
            };

            if (statistics.Servers == 0)
            {
                return Task.FromResult(
                    HealthCheckResult.Unhealthy(
                        "No Hangfire servers running",
                        data: data
                    )
                );
            }

            if (statistics.Failed > 100)
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        $"High number of failed jobs: {statistics.Failed}",
                        data: data
                    )
                );
            }

            return Task.FromResult(
                HealthCheckResult.Healthy("Hangfire is healthy", data: data)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hangfire health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Hangfire health check failed",
                    exception: ex
                )
            );
        }
    }
}
