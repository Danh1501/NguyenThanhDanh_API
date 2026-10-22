using Microsoft.EntityFrameworkCore;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
public sealed class EmployeeRepository(ApplicationDbContext db) : IEmployeeRepository
{
    public async Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default) => await db.Employees.AsNoTracking().OrderBy(x => x.FullName).ToListAsync(ct);
    public Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default) => db.Employees.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<Employee?> GetByUserIdAsync(string userId, CancellationToken ct = default) => db.Employees.FirstOrDefaultAsync(x => x.IdentityUserId == userId, ct);
    public async Task<Employee> AddAsync(Employee employee, CancellationToken ct = default) { db.Employees.Add(employee); await db.SaveChangesAsync(ct); return employee; }
    public async Task UpdateAsync(Employee employee, CancellationToken ct = default) { db.Employees.Update(employee); await db.SaveChangesAsync(ct); }
    public async Task DeleteAsync(Employee employee, CancellationToken ct = default) { db.Employees.Remove(employee); await db.SaveChangesAsync(ct); }
}