using System.ComponentModel.DataAnnotations;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
public sealed class RegisterRequest
{
    [Required, MinLength(3), MaxLength(50)] public string Username { get; init; } = string.Empty;
    [Required, EmailAddress] public string Email { get; init; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; init; } = string.Empty;
    [Required, MaxLength(150)] public string FullName { get; init; } = string.Empty;
    [MaxLength(100)] public string Department { get; init; } = string.Empty;
    [Required, RegularExpression("^(Admin|Employee)$", ErrorMessage = "Role chỉ nhận Admin hoặc Employee.")]
    public string Role { get; init; } = "Employee";
}
