using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
public interface IAttendanceRepository
{
    Task<IReadOnlyList<AttendanceRecord>> GetAllAsync(int? employeeId = null, CancellationToken ct = default);
    Task<AttendanceRecord?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AttendanceRecord?> GetOpenRecordAsync(int employeeId, DateOnly day, CancellationToken ct = default);
    Task<AttendanceRecord> AddAsync(AttendanceRecord record, CancellationToken ct = default);
    Task UpdateAsync(AttendanceRecord record, CancellationToken ct = default);
    Task DeleteAsync(AttendanceRecord record, CancellationToken ct = default);
}