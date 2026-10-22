using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Services;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Controllers;
[ApiController, Route("api/attendance"), Authorize]
public sealed class AttendanceController(IAttendanceRepository records, IEmployeeRepository employees, AttendanceService attendanceService) : ControllerBase
{
    [HttpGet, Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll(int? employeeId, CancellationToken ct) => Ok(await records.GetAllAsync(employeeId, ct));
    [HttpGet("my"), Authorize(Roles = "Employee")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var employee = await CurrentEmployee(ct);
        return employee is null ? NotFound(new { message = "Tài khoản chưa được liên kết hồ sơ nhân viên." }) : Ok(await records.GetAllAsync(employee.Id, ct));
    }
    [HttpGet("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) => await records.GetByIdAsync(id, ct) is { } item ? Ok(item) : NotFound();
    [HttpPost("check-in"), Authorize(Roles = "Employee")]
    public async Task<IActionResult> CheckIn(AttendanceRequest request, CancellationToken ct)
    {
        var employee = await CurrentEmployee(ct);
        if (employee is null) return NotFound(new { message = "Tài khoản chưa có hồ sơ nhân viên." });
        AttendanceRecord item;
        try { item = await attendanceService.CheckInAsync(employee, request, ct); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }
    [HttpPost("check-out"), Authorize(Roles = "Employee")]
    public async Task<IActionResult> CheckOut(CancellationToken ct)
    {
        var employee = await CurrentEmployee(ct);
        if (employee is null) return NotFound();
        var item = await attendanceService.CheckOutAsync(employee.Id, ct);
        if (item is null) return NotFound(new { message = "Chưa có lượt chấm công vào đang mở." });
        return Ok(item);
    }
    [HttpPut("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateAttendanceRequest request, CancellationToken ct)
    {
        var item = await records.GetByIdAsync(id, ct); if (item is null) return NotFound();
        if (request.Status is not null) item.Status = request.Status;
        if (request.Note is not null) item.Note = request.Note;
        await records.UpdateAsync(item, ct); return Ok(item);
    }
    [HttpDelete("{id:int}"), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var item = await records.GetByIdAsync(id, ct); if (item is null) return NotFound();
        await records.DeleteAsync(item, ct); return NoContent();
    }
    private Task<Employee?> CurrentEmployee(CancellationToken ct) => employees.GetByUserIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "", ct);
}
