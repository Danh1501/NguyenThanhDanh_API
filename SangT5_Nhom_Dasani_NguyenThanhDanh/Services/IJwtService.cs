using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
public interface IJwtService { Task<string> CreateAsync(ApplicationUser user); }