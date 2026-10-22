using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

public interface IGitHubAuthService
{
    Task<(bool Ok, string Message, string? Token, string? Role, string? Name, string? Email)> SignInAsync(
        GitHubLoginRequest request,
        CancellationToken cancellationToken = default);
}
