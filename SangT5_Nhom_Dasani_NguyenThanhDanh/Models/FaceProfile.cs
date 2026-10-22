namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Models;

/// <summary>Chỉ lưu vector đặc trưng; không lưu ảnh khuôn mặt.</summary>
public sealed class FaceProfile
{
    public string UserId { get; set; } = string.Empty;
    public string VectorJson { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ApplicationUser? User { get; set; }
}
