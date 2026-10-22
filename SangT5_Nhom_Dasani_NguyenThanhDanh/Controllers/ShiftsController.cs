using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Controllers;
[ApiController, Route("api/shifts"), Authorize(Roles = "Admin")]
public sealed class ShiftsController(IShiftRepository shifts) : ControllerBase
{
    [HttpGet] public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await shifts.GetAllAsync(ct));
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id, CancellationToken ct) => await shifts.GetByIdAsync(id, ct) is { } shift ? Ok(shift) : NotFound();
    [HttpPost] public async Task<IActionResult> Create(ShiftRequest request, CancellationToken ct) { var shift = await shifts.AddAsync(new Shift { Name = request.Name, StartTime = request.StartTime, EndTime = request.EndTime, IsActive = request.IsActive }, ct); return CreatedAtAction(nameof(GetById), new { id = shift.Id }, shift); }
    [HttpPut("{id:int}")] public async Task<IActionResult> Update(int id, ShiftRequest request, CancellationToken ct) { var shift = await shifts.GetByIdAsync(id, ct); if (shift is null) return NotFound(); shift.Name = request.Name; shift.StartTime = request.StartTime; shift.EndTime = request.EndTime; shift.IsActive = request.IsActive; await shifts.UpdateAsync(shift, ct); return NoContent(); }
    [HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id, CancellationToken ct) { var shift = await shifts.GetByIdAsync(id, ct); if (shift is null) return NotFound(); await shifts.DeleteAsync(shift, ct); return NoContent(); }
}