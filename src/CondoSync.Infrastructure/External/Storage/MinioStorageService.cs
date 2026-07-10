using CondoSync.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;

namespace CondoSync.Infrastructure.External.Storage;

public class MinioStorageService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _defaultBucket;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(string endpoint, string accessKey, string secretKey, string defaultBucket, ILogger<MinioStorageService> logger)
    {
        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();

        _defaultBucket = defaultBucket;
        _logger = logger;
    }

    public async Task<string> UploadAsync(string bucketName, string objectName, Stream data, string contentType, CancellationToken cancellationToken = default)
    {
        var bucket = bucketName ?? _defaultBucket;

        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucket), cancellationToken);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucket), cancellationToken);
            _logger.LogInformation("Bucket {BucketName} criado", bucket);
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);

        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

        _logger.LogDebug("Arquivo {ObjectName} enviado para bucket {BucketName}", objectName, bucket);

        return $"{bucket}/{objectName}";
    }

    public async Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var bucket = bucketName ?? _defaultBucket;
        var memoryStream = new MemoryStream();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(memoryStream));

        await _minioClient.GetObjectAsync(getObjectArgs, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task DeleteAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        var bucket = bucketName ?? _defaultBucket;

        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName);

        await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);

        _logger.LogDebug("Arquivo {ObjectName} removido do bucket {BucketName}", objectName, bucket);
    }

    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, int expiryInSeconds = 3600, CancellationToken cancellationToken = default)
    {
        var bucket = bucketName ?? _defaultBucket;

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds);

        return await _minioClient.PresignedGetObjectAsync(args);
    }
}
