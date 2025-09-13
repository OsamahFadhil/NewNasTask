using Insight.Invoicing.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace Insight.Invoicing.Infrastructure.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioService> _logger;
    private readonly string _defaultBucket;

    public MinioService(IMinioClient minioClient, IConfiguration configuration, ILogger<MinioService> logger)
    {
        _minioClient = minioClient;
        _logger = logger;
        _defaultBucket = configuration.GetValue<string>("MinIO:DefaultBucket") ?? "invoicing-files";
    }

    public async Task<FileReference> UploadFileAsync(
        string bucketName,
        string objectName,
        byte[] fileContent,
        string contentType,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(bucketName, cancellationToken);

            using var stream = new MemoryStream(fileContent);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(fileContent.Length)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation(
                "Successfully uploaded file {OriginalFileName} as {ObjectName} to bucket {BucketName}",
                originalFileName, objectName, bucketName);

            return new FileReference(bucketName, objectName, originalFileName, fileContent.Length, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to upload file {OriginalFileName} as {ObjectName} to bucket {BucketName}",
                originalFileName, objectName, bucketName);
            throw;
        }
    }

    public async Task<string> GetPresignedUrlAsync(
        string bucketName,
        string objectName,
        int expiryInHours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithExpiry(60 * 60 * expiryInHours); // Convert hours to seconds

            var presignedUrl = await _minioClient.PresignedGetObjectAsync(args);

            _logger.LogInformation(
                "Generated presigned URL for {ObjectName} in bucket {BucketName} with {ExpiryHours} hours expiry",
                objectName, bucketName, expiryInHours);

            return presignedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to generate presigned URL for {ObjectName} in bucket {BucketName}",
                objectName, bucketName);
            throw;
        }
    }

    public async Task<byte[]> DownloadFileAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);

            _logger.LogInformation(
                "Successfully downloaded {ObjectName} from bucket {BucketName}",
                objectName, bucketName);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to download {ObjectName} from bucket {BucketName}",
                objectName, bucketName);
            throw;
        }
    }

    public async Task DeleteFileAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted {ObjectName} from bucket {BucketName}",
                objectName, bucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to delete {ObjectName} from bucket {BucketName}",
                objectName, bucketName);
            throw;
        }
    }

    public async Task EnsureBucketExistsAsync(
        string bucketName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
            var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (!bucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);

                _logger.LogInformation("Created bucket {BucketName}", bucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure bucket {BucketName} exists", bucketName);
            throw;
        }
    }

    public async Task<(bool Exists, long Size, DateTime LastModified)> GetFileInfoAsync(
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);

            var objectStat = await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);

            return (true, objectStat.Size, objectStat.LastModified);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Could not get file info for {ObjectName} in bucket {BucketName}",
                objectName, bucketName);

            return (false, 0, DateTime.MinValue);
        }
    }
}
