using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Repositories;
public interface IEmployeeRepository
{
    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Employee?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Employee> AddAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task DeleteAsync(Employee employee, CancellationToken ct = default);
}