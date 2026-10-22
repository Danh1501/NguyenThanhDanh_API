using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
public interface IAuthService
{
    Task<(bool Ok, string Message)> RegisterAsync(RegisterRequest request);
    Task<(bool Ok, string Message)> LoginAsync(LoginRequest request);
    Task<(bool Ok, string Message, string? Token, string? Role, string? Name, string? Username, string? Email)> VerifyOtpAsync(VerifyOtpRequest request);
}
