using Microsoft.EntityFrameworkCore;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
public sealed class ShiftRepository(ApplicationDbContext db) : IShiftRepository
{
    public async Task<IReadOnlyList<Shift>> GetAllAsync(CancellationToken ct = default) => await db.Shifts.AsNoTracking().OrderBy(x => x.StartTime).ToListAsync(ct);
    public Task<Shift?> GetByIdAsync(int id, CancellationToken ct = default) => db.Shifts.FirstOrDefaultAsync(x => x.Id == id, ct);
    public async Task<Shift> AddAsync(Shift shift, CancellationToken ct = default) { db.Shifts.Add(shift); await db.SaveChangesAsync(ct); return shift; }
    public async Task UpdateAsync(Shift shift, CancellationToken ct = default) { db.Shifts.Update(shift); await db.SaveChangesAsync(ct); }
    public async Task DeleteAsync(Shift shift, CancellationToken ct = default) { db.Shifts.Remove(shift); await db.SaveChangesAsync(ct); }
}