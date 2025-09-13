namespace Insight.Invoicing.Domain.ValueObjects;

public record FileReference
{
    public string BucketName { get; init; }

    public string ObjectName { get; init; }

    public string OriginalFileName { get; init; }

    public long FileSize { get; init; }

    public string ContentType { get; init; }

    public FileReference(string bucketName, string objectName, string originalFileName, long fileSize, string contentType)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new ArgumentException("Bucket name cannot be null or empty", nameof(bucketName));

        if (string.IsNullOrWhiteSpace(objectName))
            throw new ArgumentException("Object name cannot be null or empty", nameof(objectName));

        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name cannot be null or empty", nameof(originalFileName));

        if (fileSize <= 0)
            throw new ArgumentException("File size must be greater than zero", nameof(fileSize));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be null or empty", nameof(contentType));

        BucketName = bucketName.ToLowerInvariant();
        ObjectName = objectName;
        OriginalFileName = originalFileName;
        FileSize = fileSize;
        ContentType = contentType.ToLowerInvariant();
    }

    public string GetFullPath() => $"{BucketName}/{ObjectName}";

    public bool IsImage() => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    public bool IsPdf() => ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);

    public string GetFileSizeString()
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = FileSize;
        int order = 0;

        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    public override string ToString() => $"{OriginalFileName} ({GetFileSizeString()}) at {GetFullPath()}";
}

