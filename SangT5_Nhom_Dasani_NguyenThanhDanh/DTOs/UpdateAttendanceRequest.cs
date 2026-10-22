using System.ComponentModel.DataAnnotations;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
public sealed class UpdateAttendanceRequest
{
    [MaxLength(30)] public string? Status { get; init; }
    [MaxLength(500)] public string? Note { get; init; }
}