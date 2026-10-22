namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
public interface IOtpService
{
    Task CreateAndSendAsync(string userId, string email, CancellationToken ct = default);
    bool Verify(string userId, string code);
}
