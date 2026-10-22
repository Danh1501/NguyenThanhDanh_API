using System.Collections.Concurrent;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
public sealed class OtpService(IConfiguration config, ILogger<OtpService> logger) : IOtpService
{
    private sealed record Challenge(string Code, DateTimeOffset ExpiresAt);
    private readonly ConcurrentDictionary<string, Challenge> challenges = new();
    public async Task CreateAndSendAsync(string userId, string email, CancellationToken ct = default)
    {
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var expiry = config.GetValue("Otp:ExpiryMinutes", 5);
        var host = config["Smtp:Host"];
        var username = config["Smtp:Username"];
        var password = config["Smtp:Password"];
        var from = config["Smtp:From"];
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from))
            throw new InvalidOperationException("Máy chủ chưa được cấu hình gửi email OTP. Hãy cấu hình SMTP trong User Secrets.");

        using var message = new MailMessage(from, email)
        {
            Subject = "Mã xác minh chấm công",
            Body = $"Mã OTP của bạn là {code}. Mã có hiệu lực trong {expiry} phút.",
            IsBodyHtml = false
        };
        using var client = new SmtpClient(host, config.GetValue("Smtp:Port", 587))
        {
            EnableSsl = config.GetValue("Smtp:EnableSsl", true),
            Credentials = new NetworkCredential(username, password)
        };
        await client.SendMailAsync(message, ct);
        logger.LogInformation("OTP email sent to {Email}", email);
        challenges[userId] = new Challenge(code, DateTimeOffset.UtcNow.AddMinutes(expiry));
    }
    public bool Verify(string userId, string code)
    {
        if (!challenges.TryGetValue(userId, out var challenge) || challenge.ExpiresAt < DateTimeOffset.UtcNow) return false;
        if (!string.Equals(challenge.Code, code, StringComparison.Ordinal)) return false;
        challenges.TryRemove(userId, out _);
        return true;
    }
}
