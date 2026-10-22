using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Controllers;
[ApiController, Route("api/employees"), Authorize(Roles = "Admin")]
public sealed class EmployeesController(IEmployeeRepository employees, UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await employees.GetAllAsync(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct) => await employees.GetByIdAsync(id, ct) is { } item ? Ok(item) : NotFound();
    [HttpPost]
    public async Task<IActionResult> Create(EmployeeRequest request, CancellationToken ct)
    {
        if (await users.FindByEmailAsync(request.Email) is not null) return Conflict(new { message = "Email đã được sử dụng." });
        var username = string.IsNullOrWhiteSpace(request.Username) ? request.Email.Split('@')[0] : request.Username;
        if (await users.FindByNameAsync(username) is not null) return Conflict(new { message = "Tên đăng nhập đã tồn tại." });
        var account = new ApplicationUser { UserName = username, Email = request.Email, EmailConfirmed = true, FullName = request.FullName };
        if (string.IsNullOrWhiteSpace(request.TemporaryPassword)) return BadRequest(new { message = "Cần đặt mật khẩu ban đầu cho nhân viên." });
        var created = await users.CreateAsync(account, request.TemporaryPassword);
        if (!created.Succeeded) return BadRequest(new { errors = created.Errors.Select(x => x.Description) });
        if (!await roles.RoleExistsAsync("Employee")) await roles.CreateAsync(new IdentityRole("Employee"));
        await users.AddToRoleAsync(account, "Employee");
        var employee = await employees.AddAsync(new Employee { IdentityUserId = account.Id, FullName = request.FullName, Email = request.Email, Department = request.Department, Position = request.Position, IsActive = request.IsActive }, ct);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, EmployeeRequest request, CancellationToken ct)
    {
        var employee = await employees.GetByIdAsync(id, ct);
        if (employee is null) return NotFound();
        var account = await users.FindByIdAsync(employee.IdentityUserId);
        var duplicateEmail = await users.FindByEmailAsync(request.Email);
        if (duplicateEmail is not null && duplicateEmail.Id != employee.IdentityUserId)
            return Conflict(new { message = "Email đã được tài khoản khác sử dụng." });
        if (account is not null)
        {
            account.Email = request.Email;
            account.UserName = string.IsNullOrWhiteSpace(request.Username) ? account.UserName : request.Username;
            account.FullName = request.FullName;
            var updateResult = await users.UpdateAsync(account);
            if (!updateResult.Succeeded)
                return BadRequest(new { errors = updateResult.Errors.Select(error => error.Description) });
        }
        employee.FullName = request.FullName;
        employee.Email = request.Email;
        employee.Department = request.Department;
        employee.Position = request.Position;
        employee.IsActive = request.IsActive;
        await employees.UpdateAsync(employee, ct);
        return NoContent();
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var employee = await employees.GetByIdAsync(id, ct);
        if (employee is null) return NotFound();
        var account = await users.FindByIdAsync(employee.IdentityUserId);
        if (account is not null) await users.DeleteAsync(account); else await employees.DeleteAsync(employee, ct);
        return NoContent();
    }
}
