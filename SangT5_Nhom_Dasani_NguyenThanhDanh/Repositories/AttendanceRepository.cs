using Microsoft.EntityFrameworkCore;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
public sealed class AttendanceRepository(ApplicationDbContext db) : IAttendanceRepository
{
    public async Task<IReadOnlyList<AttendanceRecord>> GetAllAsync(int? employeeId = null, CancellationToken ct = default)
    {
        var query = db.AttendanceRecords.AsNoTracking().Include(x => x.Employee).AsQueryable();
        if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId.Value);
        return await query.OrderByDescending(x => x.CheckInAt).ToListAsync(ct);
    }
    public Task<AttendanceRecord?> GetByIdAsync(int id, CancellationToken ct = default) => db.AttendanceRecords.Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<AttendanceRecord?> GetOpenRecordAsync(int employeeId, DateOnly day, CancellationToken ct = default)
    {
        var localStart = DateTime.SpecifyKind(day.ToDateTime(TimeOnly.MinValue), DateTimeKind.Local);
        var start = new DateTimeOffset(localStart);
        return db.AttendanceRecords.FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.CheckOutAt == null && x.CheckInAt >= start && x.CheckInAt < start.AddDays(1), ct);
    }
    public async Task<AttendanceRecord> AddAsync(AttendanceRecord record, CancellationToken ct = default) { db.AttendanceRecords.Add(record); await db.SaveChangesAsync(ct); return record; }
    public async Task UpdateAsync(AttendanceRecord record, CancellationToken ct = default) { db.AttendanceRecords.Update(record); await db.SaveChangesAsync(ct); }
    public async Task DeleteAsync(AttendanceRecord record, CancellationToken ct = default) { db.AttendanceRecords.Remove(record); await db.SaveChangesAsync(ct); }
}