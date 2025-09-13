using Insight.Invoicing.Domain.ValueObjects;

namespace Insight.Invoicing.Infrastructure.Services;

public interface IMinioService
{
    Task<FileReference> UploadFileAsync(
        string bucketName,
        string objectName,
        byte[] fileContent,
        string contentType,
        string originalFileName,
        CancellationToken cancellationToken = default);

    Task<string> GetPresignedUrlAsync(
        string bucketName,
        string objectName,
        int expiryInHours = 24,
        CancellationToken cancellationToken = default);

    Task<byte[]> DownloadFileAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default);

    Task EnsureBucketExistsAsync(
        string bucketName,
        CancellationToken cancellationToken = default);

    Task<(bool Exists, long Size, DateTime LastModified)> GetFileInfoAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default);
}

