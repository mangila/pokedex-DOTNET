﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using pokedex_shared.Model.Document.Embedded;
using pokedex_shared.Model.Domain;
using pokedex_shared.Option;
using static MongoDB.Driver.Builders<MongoDB.Driver.GridFS.GridFSFileInfo>;

namespace pokedex_shared.Service.Command;

public class MongoDbGridFsCommandService
{
    private readonly ILogger<MongoDbGridFsCommandService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GridFSBucket _bucket;
    private readonly PokedexApiOption _pokedexApiOption;

    public MongoDbGridFsCommandService(
        ILogger<MongoDbGridFsCommandService> logger,
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
        string? getFileUri;
        string? src;
        if (fileInfo is not null)
        {
            _logger.LogInformation("GridFs hit - {fileName}", fileName);
            getFileUri = _pokedexApiOption.GetFileUri.Replace("{id}", fileInfo.Id.ToString());
            src = $"{_pokedexApiOption.Url}/{getFileUri}";
            return new PokemonMediaDocument(
                MediaId: fileInfo.Id.ToString(),
                FileName: fileInfo.Filename,
                ContentType: fileInfo.Metadata["content_type"].AsString,
                Src: src
            );
        }

        _logger.LogInformation("GridFs miss - {fileName}", fileName);
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
        getFileUri = _pokedexApiOption.GetFileUri.Replace("{id}", mediaId.ToString());
        src = $"{_pokedexApiOption.Url}/{getFileUri}";
        return new PokemonMediaDocument(
            MediaId: mediaId.ToString(),
            FileName: fileName,
            ContentType: entry.GetContentType(),
            Src: src
        );
    }
}