using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Controllers;

[ApiController, Route("api/face"), Authorize]
public sealed class FaceController(IFaceRecognitionService faces) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return Ok(new { enrolled = await faces.IsEnrolledAsync(userId, cancellationToken) });
    }

    [HttpDelete("profile")]
    public async Task<IActionResult> DeleteProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        await faces.DeleteAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("enroll"), Consumes("multipart/form-data")]
    public async Task<IActionResult> Enroll([FromForm] List<IFormFile> images, CancellationToken cancellationToken)
    {
        if (images.Count is < 3 or > 5 || images.Any(image => image.Length == 0 || image.Length > 2 * 1024 * 1024))
            return BadRequest(new { message = "Chụp và gửi từ 3 đến 5 ảnh JPEG/PNG, mỗi ảnh tối đa 2 MB." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        try
        {
            var streams = images.Select(image => image.OpenReadStream()).ToList();
            try { await faces.EnrollAsync(userId, streams, cancellationToken); }
            finally { foreach (var stream in streams) await stream.DisposeAsync(); }
            return Ok(new { success = true, message = "Đã đăng ký khuôn mặt. Ảnh gốc không được lưu." });
        }
        catch (InvalidDataException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [HttpPost("verify"), Consumes("multipart/form-data")]
    public async Task<IActionResult> Verify([FromForm] IFormFile image, CancellationToken cancellationToken)
    {
        if (image is null || image.Length == 0 || image.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "Thiếu ảnh hoặc ảnh vượt quá 2 MB." });
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        try
        {
            await using var stream = image.OpenReadStream();
            var matched = await faces.VerifyAsync(userId, stream, cancellationToken);
            return matched ? Ok(new { matched = true }) : Unauthorized(new { matched = false, message = "Khuôn mặt không khớp." });
        }
        catch (InvalidDataException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
