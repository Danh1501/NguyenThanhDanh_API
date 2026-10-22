using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
public sealed class AuthService(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, ApplicationDbContext db, IOtpService otp, IJwtService jwt, ILogger<AuthService> logger) : IAuthService
{
    public async Task<(bool Ok, string Message)> RegisterAsync(RegisterRequest request)
    {
        if (await users.FindByNameAsync(request.Username) is not null) return (false, "Tên đăng nhập đã tồn tại.");
        if (await users.FindByEmailAsync(request.Email) is not null) return (false, "Email đã được sử dụng.");
        var role = request.Role.Trim();
        if (role is not ("Admin" or "Employee")) return (false, "Vai trò không hợp lệ.");
        var user = new ApplicationUser { UserName = request.Username, Email = request.Email, FullName = request.FullName, EmailConfirmed = true };
        var result = await users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return (false, string.Join(" ", result.Errors.Select(x => x.Description)));
        if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole(role));
        await users.AddToRoleAsync(user, role);
        if (role == "Employee")
        {
            db.Employees.Add(new Employee { IdentityUserId = user.Id, FullName = user.FullName, Email = user.Email!, Department = request.Department, Position = "Nhân viên" });
            await db.SaveChangesAsync();
        }
        return (true, "Đăng ký thành công. Hãy đăng nhập để nhận mã OTP qua email.");
    }
    public async Task<(bool Ok, string Message)> LoginAsync(LoginRequest request)
    {
        var user = await users.FindByNameAsync(request.Username);
        if (user is null || !await users.CheckPasswordAsync(user, request.Password)) return (false, "Tên đăng nhập hoặc mật khẩu không đúng.");
        if (await db.Employees.AnyAsync(employee => employee.IdentityUserId == user.Id && !employee.IsActive))
            return (false, "Tài khoản nhân viên đã bị vô hiệu hoá. Hãy liên hệ quản trị viên.");
        try { await otp.CreateAndSendAsync(user.Id, user.Email ?? ""); }
        catch (Exception ex) when (ex is InvalidOperationException or System.Net.Mail.SmtpException)
        {
            logger.LogError(ex, "Could not send login OTP email for user {Username}", request.Username);
            return (false, "Không gửi được email OTP. Kiểm tra cấu hình SMTP và cửa sổ Output của Visual Studio.");
        }
        return (true, "Mã OTP đã được gửi đến email đăng ký.");
    }
    public async Task<(bool Ok, string Message, string? Token, string? Role, string? Name, string? Username, string? Email)> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await users.FindByNameAsync(request.Username);
        if (user is null || !otp.Verify(user.Id, request.Code)) return (false, "OTP không hợp lệ hoặc đã hết hạn.", null, null, null, null, null);
        var userRoles = await users.GetRolesAsync(user);
        var role = userRoles.FirstOrDefault() ?? "Employee";
        return (true, "Đăng nhập thành công.", await jwt.CreateAsync(user), role, user.FullName, user.UserName, user.Email);
    }
}
