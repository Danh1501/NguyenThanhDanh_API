using Microsoft.AspNetCore.Identity;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
public sealed class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public Employee? Employee { get; set; }
}