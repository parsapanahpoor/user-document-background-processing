using UserDocumentAPI.Data;

namespace UserDocumentAPI.BackgroundJobs;

public class CompletionMessageJob
{
    private readonly ILogger<CompletionMessageJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public CompletionMessageJob(ILogger<CompletionMessageJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SendCompletionMessageAsync(Guid userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return;
        }

        _logger.LogInformation("📧 Sending completion message to {Email}", user.Email);
        
        await Task.Delay(500);
        
        _logger.LogInformation("✅ Completion message: Your document has been processed and is now available. ({Email})", user.Email);
    }
}
