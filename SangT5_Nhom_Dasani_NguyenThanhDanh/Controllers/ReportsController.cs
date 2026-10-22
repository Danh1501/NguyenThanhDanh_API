using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Controllers;

[ApiController, Route("api/reports"), Authorize(Roles = "Admin")]
public sealed class ReportsController(IAttendanceRepository attendance) : ControllerBase
{
    [HttpGet("attendance")]
    public async Task<IActionResult> AttendanceReport(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var startDate = from ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var endDate = to ?? DateOnly.FromDateTime(DateTime.Today);
        if (startDate > endDate) return BadRequest(new { message = "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc." });

        var records = await attendance.GetAllAsync(ct: ct);
        var start = new DateTimeOffset(startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local));
        var end = new DateTimeOffset(endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Local));
        var rows = records.Where(x => x.CheckInAt >= start && x.CheckInAt < end)
            .GroupBy(x => new { x.EmployeeId, EmployeeName = x.Employee?.FullName ?? "Không rõ", Day = DateOnly.FromDateTime(x.CheckInAt.LocalDateTime) })
            .Select(g => new { g.Key.EmployeeId, g.Key.EmployeeName, Date = g.Key.Day, Count = g.Count(), CheckIns = g.Min(x => x.CheckInAt), CheckOuts = g.Max(x => x.CheckOutAt), Status = string.Join(", ", g.Select(x => x.Status).Distinct()) })
            .OrderBy(x => x.Date).ThenBy(x => x.EmployeeName);

        return Ok(new { from = startDate, to = endDate, totalRecords = rows.Sum(x => x.Count), records = rows });
    }
}
