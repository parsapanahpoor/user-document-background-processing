using UserDocumentAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace UserDocumentAPI.BackgroundJobs;

public class WelcomeMessageJob
{
    private readonly ILogger<WelcomeMessageJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public WelcomeMessageJob(ILogger<WelcomeMessageJob> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task SendWelcomeMessageAsync(Guid userId)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}. Job will be marked as completed.", userId);
            // در این حالت job را fail نمی‌کنیم چون retry کمکی نمیکنه
            // کاربر ممکنه حذف شده باشه
            return;
        }

        try
        {
            _logger.LogInformation("📧 Sending welcome message to {Email}", user.Email);

            // شبیه‌سازی ارسال ایمیل
            // در واقعیت اینجا از Email Service استفاده می‌شود
            await SimulateSendEmailAsync(user.Email, user.Name);

            _logger.LogInformation("✅ Welcome message sent to {Name} ({Email})", user.Name, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send welcome message to {Email}", user.Email);
            throw; // برای فعال‌سازی Retry Policy
        }
    }

    private async Task SimulateSendEmailAsync(string email, string name)
    {
        // شبیه‌سازی تأخیر شبکه
        await Task.Delay(500);
        
        // شبیه‌سازی خطای تصادفی برای تست retry (فقط در صورت نیاز)
        // if (Random.Shared.NextDouble() < 0.1)
        //     throw new Exception("Simulated email service failure");
    }
}
