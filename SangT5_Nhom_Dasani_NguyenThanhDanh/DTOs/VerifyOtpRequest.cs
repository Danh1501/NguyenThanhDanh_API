using System.ComponentModel.DataAnnotations;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
public sealed class VerifyOtpRequest
{
    [Required] public string Username { get; init; } = string.Empty;
    [Required, MinLength(6), MaxLength(6)] public string Code { get; init; } = string.Empty;
}