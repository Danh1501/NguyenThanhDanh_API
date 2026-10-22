using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
public static class DatabaseSeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("IF OBJECT_ID(N'dbo.FaceProfiles', N'U') IS NULL CREATE TABLE dbo.FaceProfiles (UserId nvarchar(450) NOT NULL CONSTRAINT PK_FaceProfiles PRIMARY KEY, VectorJson nvarchar(max) NOT NULL, UpdatedAtUtc datetime2 NOT NULL, CONSTRAINT FK_FaceProfiles_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE)");
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Employee" })
            if (!await roleManager.RoleExistsAsync(role)) await roleManager.CreateAsync(new IdentityRole(role));
    }
}
