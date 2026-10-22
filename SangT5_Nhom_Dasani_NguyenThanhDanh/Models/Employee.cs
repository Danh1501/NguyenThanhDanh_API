using System.Text.Json.Serialization;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
public sealed class Employee
{
    public int Id { get; set; }
    public string IdentityUserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ApplicationUser? User { get; set; }
    [JsonIgnore]
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
