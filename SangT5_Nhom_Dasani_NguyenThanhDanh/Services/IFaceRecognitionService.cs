namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

public interface IFaceRecognitionService
{
    Task EnrollAsync(string userId, IReadOnlyList<Stream> images, CancellationToken cancellationToken);
    Task<bool> VerifyAsync(string userId, Stream image, CancellationToken cancellationToken);
    Task<bool> IsEnrolledAsync(string userId, CancellationToken cancellationToken);
    Task DeleteAsync(string userId, CancellationToken cancellationToken);
}
