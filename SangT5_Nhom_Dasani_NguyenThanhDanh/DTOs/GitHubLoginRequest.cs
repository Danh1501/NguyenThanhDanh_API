using System.ComponentModel.DataAnnotations;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;

public sealed class GitHubLoginRequest
{
    [Required, StringLength(500)]
    public string Code { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 43)]
    public string CodeVerifier { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string RedirectUri { get; init; } = string.Empty;
}
