using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
public interface IShiftRepository
{
    Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken ct = default);
    Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Shift> AddAsync(Shift shift, CancellationToken ct = default);
    Task UpdateAsync(Shift shift, CancellationToken ct = default);
    Task DeleteAsync(Shift shift, CancellationToken ct = default);
}