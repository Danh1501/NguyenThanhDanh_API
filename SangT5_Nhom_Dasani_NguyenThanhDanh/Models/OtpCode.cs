namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Models;

public sealed class OtpCode
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
