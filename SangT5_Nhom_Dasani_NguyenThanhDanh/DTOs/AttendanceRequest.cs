using System.ComponentModel.DataAnnotations;
namespace SangT5_Nhom_Dasani_NguyenThanhDanh.DTOs;
public sealed class AttendanceRequest
{
    [Required, Range(-90, 90)]
    public double? Latitude { get; init; }

    [Required, Range(-180, 180)]
    public double? Longitude { get; init; }
}
