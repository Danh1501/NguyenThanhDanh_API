using System.Text.Json;
using FaceAiSharp;
using FaceAiSharp.Extensions;
using Microsoft.EntityFrameworkCore;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Data;
using SangT5_Nhom_Dasani_NguyenThanhDanh.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SangT5_Nhom_Dasani_NguyenThanhDanh.Services;

/// <summary>Nhận diện 1:1 trên backend. Chỉ lưu vector đặc trưng, không lưu ảnh gốc.</summary>
public sealed class FaceRecognitionService(ApplicationDbContext db, IConfiguration configuration) : IFaceRecognitionService
{
    private const int MaxImageBytes = 2 * 1024 * 1024;
    private static readonly Lazy<(IFaceDetectorWithLandmarks Detector, IFaceEmbeddingsGenerator Embedder)> Models = new(() =>
        (FaceAiSharpBundleFactory.CreateFaceDetectorWithLandmarks(), FaceAiSharpBundleFactory.CreateFaceEmbeddingsGenerator()));
    private static readonly SemaphoreSlim ModelGate = new(1, 1);

    public async Task EnrollAsync(string userId, IReadOnlyList<Stream> images, CancellationToken cancellationToken)
    {
        if (images.Count is < 3 or > 5) throw new InvalidDataException("Cần chụp từ 3 đến 5 ảnh khuôn mặt.");
        var vectors = new List<float[]>();
        foreach (var image in images) vectors.Add(await GetVectorAsync(image, cancellationToken));

        var mean = new float[vectors[0].Length];
        foreach (var vector in vectors)
            for (var i = 0; i < mean.Length; i++) mean[i] += vector[i];
        Normalize(mean);
        if (vectors.Any(vector => Similarity(vector, mean) < 0.30))
            throw new InvalidDataException("Các ảnh khuôn mặt chưa nhất quán. Hãy chụp lại ảnh của cùng một người.");

        var profile = await db.FaceProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (profile is null)
        {
            profile = new FaceProfile { UserId = userId };
            db.FaceProfiles.Add(profile);
        }
        profile.VectorJson = JsonSerializer.Serialize(mean);
        profile.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> VerifyAsync(string userId, Stream image, CancellationToken cancellationToken)
    {
        var vectorJson = await db.FaceProfiles.AsNoTracking().Where(x => x.UserId == userId)
            .Select(x => x.VectorJson).FirstOrDefaultAsync(cancellationToken);
        if (vectorJson is null) throw new InvalidDataException("Tài khoản chưa đăng ký khuôn mặt.");
        var enrolled = JsonSerializer.Deserialize<float[]>(vectorJson);
        if (enrolled is null || enrolled.Length == 0) return false;
        var probe = await GetVectorAsync(image, cancellationToken);
        return Similarity(probe, enrolled) >= configuration.GetValue("Face:MatchThreshold", 0.45);
    }

    public Task<bool> IsEnrolledAsync(string userId, CancellationToken cancellationToken) =>
        db.FaceProfiles.AsNoTracking().AnyAsync(x => x.UserId == userId, cancellationToken);

    public async Task DeleteAsync(string userId, CancellationToken cancellationToken)
    {
        var profile = await db.FaceProfiles.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (profile is null) return;
        db.FaceProfiles.Remove(profile);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task<float[]> GetVectorAsync(Stream source, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int count;
        while ((count = await source.ReadAsync(chunk, cancellationToken)) > 0)
        {
            buffer.Write(chunk, 0, count);
            if (buffer.Length > MaxImageBytes) throw new InvalidDataException("Ảnh quá lớn (tối đa 2 MB).");
        }
        if (buffer.Length == 0) throw new InvalidDataException("Ảnh khuôn mặt không hợp lệ.");

        await ModelGate.WaitAsync(cancellationToken);
        try
        {
            return await Task.Run(() => Compute(buffer.ToArray()), cancellationToken);
        }
        finally { ModelGate.Release(); }
    }

    private static float[] Compute(byte[] bytes)
    {
        var format = Image.DetectFormat(bytes);
        if (format is not (JpegFormat or PngFormat)) throw new InvalidDataException("Ảnh phải có định dạng JPEG hoặc PNG.");
        using var image = Image.Load<Rgb24>(bytes);
        image.Mutate(context => context.AutoOrient());
        var longSide = Math.Max(image.Width, image.Height);
        if (longSide > 1600)
        {
            var scale = 1600d / longSide;
            image.Mutate(context => context.Resize((int)(image.Width * scale), (int)(image.Height * scale)));
        }

        var models = Models.Value;
        var faces = models.Detector.DetectFaces(image);
        if (faces.Count == 0) throw new InvalidDataException("Không tìm thấy khuôn mặt. Hãy nhìn thẳng vào camera và đủ sáng.");
        if (faces.Count > 1) throw new InvalidDataException("Ảnh chỉ được có một khuôn mặt.");
        var landmarks = faces.First().Landmarks;
        if (landmarks is null || landmarks.Count != 5) throw new InvalidDataException("Không nhận diện rõ các điểm trên khuôn mặt.");

        using var aligned = image.Clone();
        models.Embedder.AlignFaceUsingLandmarks(aligned, landmarks);
        var vector = models.Embedder.GenerateEmbedding(aligned);
        Normalize(vector);
        return vector;
    }

    private static double Similarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0) return 0;
        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return normA == 0 || normB == 0 ? 0 : dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private static void Normalize(float[] vector)
    {
        var norm = Math.Sqrt(vector.Sum(value => (double)value * value));
        if (norm == 0) return;
        for (var i = 0; i < vector.Length; i++) vector[i] = (float)(vector[i] / norm);
    }
}
