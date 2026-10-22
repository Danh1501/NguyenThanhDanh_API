using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

public sealed class GitHubAuthService(
    IHttpClientFactory clients,
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole> roles,
    ApplicationDbContext db,
    IJwtService jwt,
    IConfiguration configuration,
    ILogger<GitHubAuthService> logger) : IGitHubAuthService
{
    public async Task<(bool Ok, string Message, string? Token, string? Role, string? Name, string? Email)> SignInAsync(
        GitHubLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var clientId = configuration["GitHub:ClientId"];
        var clientSecret = configuration["GitHub:ClientSecret"];
        var configuredRedirect = configuration["GitHub:RedirectUri"];
        if (string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret) ||
            string.IsNullOrWhiteSpace(configuredRedirect))
            return Fail("Backend chưa cấu hình GitHub OAuth. Xem GITHUB_SIGN_IN_SETUP.md.");

        if (!string.Equals(request.RedirectUri, configuredRedirect, StringComparison.Ordinal))
            return Fail("GitHub redirect URI không khớp cấu hình.");

        try
        {
            var client = clients.CreateClient("GitHubOAuth");
            using var tokenResponse = await client.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["code"] = request.Code,
                    ["redirect_uri"] = configuredRedirect,
                    ["code_verifier"] = request.CodeVerifier
                }),
                cancellationToken);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("GitHub token exchange returned HTTP {StatusCode}.", tokenResponse.StatusCode);
                return Fail("GitHub không xác thực được mã đăng nhập. Hãy thử lại.");
            }

            using var tokenJson = JsonDocument.Parse(await tokenResponse.Content.ReadAsStringAsync(cancellationToken));
            if (!tokenJson.RootElement.TryGetProperty("access_token", out var accessTokenElement))
                return Fail("GitHub không cấp access token. Kiểm tra OAuth App và thử lại.");
            var accessToken = accessTokenElement.GetString();
            if (string.IsNullOrWhiteSpace(accessToken))
                return Fail("GitHub trả access token rỗng.");

            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            userRequest.Headers.Accept.ParseAdd("application/vnd.github+json");
            using var userResponse = await client.SendAsync(userRequest, cancellationToken);
            if (!userResponse.IsSuccessStatusCode)
                return Fail("Không đọc được tài khoản GitHub.");
            using var userJson = JsonDocument.Parse(await userResponse.Content.ReadAsStringAsync(cancellationToken));
            var root = userJson.RootElement;
            var providerId = root.GetProperty("id").GetInt64().ToString();
            var login = root.TryGetProperty("login", out var loginElement) ? loginElement.GetString() : null;
            var displayName = root.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null;

            using var emailsRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
            emailsRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            emailsRequest.Headers.Accept.ParseAdd("application/vnd.github+json");
            using var emailsResponse = await client.SendAsync(emailsRequest, cancellationToken);
            if (!emailsResponse.IsSuccessStatusCode)
                return Fail("Không đọc được email GitHub; hãy cấp quyền user:email rồi thử lại.");
            using var emailsJson = JsonDocument.Parse(await emailsResponse.Content.ReadAsStringAsync(cancellationToken));
            var email = emailsJson.RootElement.EnumerateArray()
                .Where(item => item.TryGetProperty("verified", out var verified) && verified.GetBoolean())
                .OrderByDescending(item => item.TryGetProperty("primary", out var primary) && primary.GetBoolean())
                .Select(item => item.TryGetProperty("email", out var value) ? value.GetString() : null)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

            if (string.IsNullOrWhiteSpace(providerId) || string.IsNullOrWhiteSpace(email))
                return Fail("Tài khoản GitHub cần có email đã xác minh.");

            const string provider = "GitHub";
            var user = await users.FindByLoginAsync(provider, providerId);
            var fullName = string.IsNullOrWhiteSpace(displayName) ? login ?? email : displayName;
            if (user is null)
            {
                user = await users.FindByEmailAsync(email);
                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FullName = fullName
                    };
                    var randomPassword = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)) + "aA1!";
                    var createResult = await users.CreateAsync(user, randomPassword);
                    if (!createResult.Succeeded)
                        return Fail(string.Join(" ", createResult.Errors.Select(error => error.Description)));

                    const string employeeRole = "Employee";
                    if (!await roles.RoleExistsAsync(employeeRole))
                        await roles.CreateAsync(new IdentityRole(employeeRole));
                    var roleResult = await users.AddToRoleAsync(user, employeeRole);
                    if (!roleResult.Succeeded)
                        return Fail(string.Join(" ", roleResult.Errors.Select(error => error.Description)));

                    db.Employees.Add(new Employee
                    {
                        IdentityUserId = user.Id,
                        FullName = fullName,
                        Email = email,
                        Position = "Nhân viên"
                    });
                    await db.SaveChangesAsync(cancellationToken);
                }
                else if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    var updateResult = await users.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                        return Fail(string.Join(" ", updateResult.Errors.Select(error => error.Description)));
                }

                var linkResult = await users.AddLoginAsync(user, new UserLoginInfo(provider, providerId, provider));
                if (!linkResult.Succeeded)
                    return Fail(string.Join(" ", linkResult.Errors.Select(error => error.Description)));
            }

            if (await db.Employees.AnyAsync(employee => employee.IdentityUserId == user.Id && !employee.IsActive, cancellationToken))
                return Fail("Tài khoản nhân viên đã bị vô hiệu hoá. Hãy liên hệ quản trị viên.");

            var userRoles = await users.GetRolesAsync(user);
            if (userRoles.Count == 0)
            {
                const string employeeRole = "Employee";
                if (!await roles.RoleExistsAsync(employeeRole))
                    await roles.CreateAsync(new IdentityRole(employeeRole));
                await users.AddToRoleAsync(user, employeeRole);
                userRoles = [employeeRole];
            }

            var appToken = await jwt.CreateAsync(user);
            return (true, "Đăng nhập GitHub thành công.", appToken, userRoles[0], user.FullName, email);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "GitHub OAuth sign-in failed.");
            return Fail("Đăng nhập GitHub thất bại. Hãy kiểm tra OAuth Client ID, Secret và callback URL.");
        }
    }

    private static (bool Ok, string Message, string? Token, string? Role, string? Name, string? Email) Fail(string message)
        => (false, message, null, null, null, null);
}
