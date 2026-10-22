using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Controllers;

[ApiController, Route("api/auth")]
public sealed class AuthController(
    IAuthService auth,
    IGitHubAuthService githubAuth) : ControllerBase
{
    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await auth.RegisterAsync(request);
        return result.Ok ? Ok(new { success = true, result.Message }) : BadRequest(new { success = false, result.Message });
    }

    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await auth.LoginAsync(request);
        return result.Ok ? Ok(new { success = true, otpRequired = true, result.Message }) : Unauthorized(new { success = false, result.Message });
    }

    [AllowAnonymous, HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        var result = await auth.VerifyOtpAsync(request);
        return result.Ok ? Ok(new { success = true, result.Message, token = result.Token, role = result.Role, fullName = result.Name, username = result.Username, email = result.Email }) : Unauthorized(new { success = false, result.Message });
    }

    [AllowAnonymous, HttpPost("github")]
    public async Task<IActionResult> GitHub(GitHubLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await githubAuth.SignInAsync(request, cancellationToken);
        return result.Ok
            ? Ok(new { success = true, result.Message, token = result.Token, role = result.Role, fullName = result.Name, email = result.Email })
            : Unauthorized(new { success = false, result.Message });
    }
}
