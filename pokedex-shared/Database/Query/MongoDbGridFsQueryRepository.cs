﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using pokedex_shared.Common.Option;
using pokedex_shared.Model.Dto;

namespace pokedex_shared.Database.Query;

public class MongoDbGridFsQueryRepository
{
    private readonly ILogger<MongoDbGridFsQueryRepository> _logger;
    private readonly GridFSBucket _bucket;

    public MongoDbGridFsQueryRepository(
        ILogger<MongoDbGridFsQueryRepository> logger,
        IOptions<MongoDbOption> mongoDbOption)
    {
        _logger = logger;
        var mongoDb = mongoDbOption.Value;
        var db = new MongoClient(mongoDb.ConnectionString)
            .GetDatabase(mongoDb.Database);
        _bucket = new GridFSBucket(db, new GridFSBucketOptions
        {
            BucketName = mongoDb.Bucket,
        });
    }

    public async Task<PokemonFileResult?> FindFileByIdAsync(
        ObjectId objectId,
        CancellationToken cancellationToken)
    {
        var fileInfo = await (await _bucket
                .FindAsync(Builders<GridFSFileInfo>.Filter.Eq("_id", objectId), cancellationToken: cancellationToken))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        if (fileInfo is null)
        {
            return null;
        }

        var bytes = await _bucket.DownloadAsBytesAsync(objectId, null, cancellationToken);
        if (bytes is null)
        {
            return null;
        }

        return new PokemonFileResult(
            FileName: fileInfo.Filename,
            ContentType: fileInfo.Metadata["content_type"].AsString,
            LastModified: fileInfo.UploadDateTime,
            File: bytes);
    }
}