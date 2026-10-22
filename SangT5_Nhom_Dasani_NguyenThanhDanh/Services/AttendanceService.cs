using SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

public sealed class AttendanceService(IAttendanceRepository records)
{
    public async Task<AttendanceRecord> CheckInAsync(Employee employee, AttendanceRequest request, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        if (await records.GetOpenRecordAsync(employee.Id, today, ct) is not null)
            throw new InvalidOperationException("Bạn đã chấm công vào hôm nay.");

        return await records.AddAsync(new AttendanceRecord
        {
            EmployeeId = employee.Id,
            CheckInAt = DateTimeOffset.UtcNow,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = "OnTime"
        }, ct);
    }

    public async Task<AttendanceRecord?> CheckOutAsync(int employeeId, CancellationToken ct = default)
    {
        var item = await records.GetOpenRecordAsync(employeeId, DateOnly.FromDateTime(DateTime.Now), ct);
        if (item is null) return null;
        item.CheckOutAt = DateTimeOffset.UtcNow;
        await records.UpdateAsync(item, ct);
        return item;
    }
}
