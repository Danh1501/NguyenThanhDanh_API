using System.ComponentModel.DataAnnotations;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
public sealed class EmployeeRequest
{
    [Required, MaxLength(150)] public string FullName { get; init; } = string.Empty;
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    [MaxLength(50)] public string? Username { get; init; }
    [MinLength(6)] public string? TemporaryPassword { get; init; }
    [MaxLength(100)] public string Department { get; init; } = string.Empty;
    [MaxLength(100)] public string Position { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
}