using System.Text.Json.Serialization;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
public sealed class AttendanceRecord
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTimeOffset CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    // Retain the legacy database column for existing records, but no longer expose QR data via the API.
    [JsonIgnore] public string QrCode { get; set; } = string.Empty;
    public string Status { get; set; } = "OnTime";
    public string? Note { get; set; }
}
