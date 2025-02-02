﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using pokedex_shared.Common.Option;
using pokedex_shared.Model.Document.Embedded;
using pokedex_shared.Model.Domain;
using static MongoDB.Driver.Builders<MongoDB.Driver.GridFS.GridFSFileInfo>;

namespace pokedex_shared.Database.Command;

public class MongoDbGridFsCommandRepository
{
    private readonly ILogger<MongoDbGridFsCommandRepository> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GridFSBucket _bucket;
    private readonly PokedexApiOption _pokedexApiOption;

    public MongoDbGridFsCommandRepository(
        ILogger<MongoDbGridFsCommandRepository> logger,
        IOptions<MongoDbOption> mongoDbOption,
        IOptions<PokedexApiOption> pokedexApiOption,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _pokedexApiOption = pokedexApiOption.Value;
        var mongoDb = mongoDbOption.Value;
        var db = new MongoClient(mongoDb.ConnectionString)
            .GetDatabase(mongoDb.Database);
        _bucket = new GridFSBucket(db, new GridFSBucketOptions
        {
            BucketName = mongoDb.Bucket,
        });
    }

    public async Task<PokemonMediaDocument> InsertAsync(
        PokemonMediaEntry entry,
        CancellationToken cancellationToken = default)
    {
        var fileName = entry.GetFileName();
        var filter = Filter
            .Eq(file => file.Filename, fileName);
        var fileInfo = await _bucket
            .Find(filter, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (fileInfo is not null)
        {
            return new PokemonMediaDocument(
                MediaId: fileInfo.Id.ToString(),
                FileName: fileInfo.Filename,
                ContentType: fileInfo.Metadata["content_type"].AsString,
                Src: GetSrc(fileInfo.Id.ToString())
            );
        }

        var httpClient = _httpClientFactory.CreateClient();
        using var response = await httpClient.GetAsync(entry.Uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var fileStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var mediaId = await _bucket.UploadFromStreamAsync(fileName, fileStream,
            new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "content_type", entry.GetContentType() },
                    { "description", entry.Description },
                }
            }, cancellationToken);
        return new PokemonMediaDocument(
            MediaId: mediaId.ToString(),
            FileName: fileName,
            ContentType: entry.GetContentType(),
            Src: GetSrc(mediaId.ToString())
        );
    }

    private string GetSrc(string mediaId)
    {
        return $"{_pokedexApiOption.Url}/{GetFileUri(mediaId)}";
    }

    private string GetFileUri(string mediaId)
    {
        return _pokedexApiOption.GetFileUri.Replace("{id}", mediaId);
    }
}