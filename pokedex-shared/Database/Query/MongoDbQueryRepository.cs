﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using pokedex_shared.Common.Option;
using pokedex_shared.Model.Document;
using pokedex_shared.Model.Domain;
using static MongoDB.Driver.Builders<pokedex_shared.Model.Document.PokemonSpeciesDocument>;

namespace pokedex_shared.Database.Query;

public class MongoDbQueryRepository
{
    private const string CaseInsensitiveMatching = "i";
    private readonly ILogger<MongoDbQueryRepository> _logger;
    private readonly IMongoCollection<PokemonSpeciesDocument> _collection;

    public MongoDbQueryRepository(
        ILogger<MongoDbQueryRepository> logger,
        IOptions<MongoDbOption> mongoDbOption)
    {
        _logger = logger;
        var mongoDb = mongoDbOption.Value;
        _collection = new MongoClient(mongoDb.ConnectionString)
            .GetDatabase(mongoDb.Database)
            .GetCollection<PokemonSpeciesDocument>(mongoDb.Collection);
        _collection.Indexes.CreateManyAsync(CreateIndexes([
            "name",
            "pedigree.generation",
            "varieties.types.type",
            "special.baby",
            "special.legendary",
            "special.mythical"
        ]));
    }

    private static List<CreateIndexModel<PokemonSpeciesDocument>> CreateIndexes(string[] fieldNames)
    {
        return fieldNames
            .Select(fieldName => IndexKeys.Ascending(fieldName))
            .Select(definition => new CreateIndexModel<PokemonSpeciesDocument>(definition))
            .ToList();
    }

    public async Task<PokemonSpeciesDocument> FindOneByIdAsync(
        PokemonId pokemonId,
        CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(doc => doc.Id == pokemonId.ToInt())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PokemonSpeciesDocument> FindOneByNameAsync(
        PokemonName pokemonName,
        CancellationToken cancellationToken = default)
    {
        return await _collection
            .Find(doc => doc.Name == pokemonName.Value)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<PokemonSpeciesDocument>> SearchByNameAsync(PokemonName search,
        CancellationToken cancellationToken = default)
    {
        var filter = Filter.Regex(
            doc => doc.Name,
            new BsonRegularExpression(search.Value, CaseInsensitiveMatching)
        );
        return await _collection
            .Find(filter)
            .Sort(Sort.Ascending(p => p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PokemonSpeciesDocument>> FindAllByIdsAsync(
        PokemonIdCollection pokemonIdCollection,
        CancellationToken cancellationToken = default)
    {
        var ids = pokemonIdCollection.Ids
            .Select(id => id.ToInt())
            .ToList();
        var filter = Filter
            .In(doc => doc.Id, ids);
        return await _collection
            .Find(filter)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PokemonSpeciesDocument>> SearchByGenerationAsync(
        PokemonGeneration generation,
        CancellationToken cancellationToken)
    {
        var filter = Filter
            .Where(doc => doc.Pedigree.Generation == generation.Value);
        return await _collection
            .Find(filter)
            .Sort(Sort.Ascending(p => p.Id))
            .ToListAsync(cancellationToken);
    }

    /**
     * <summary>
     *  Pagination logic
     *  1. Get total documents
     *  2. Total / PageSize = To get the total pages to Paginate with
     *  3. Skip documents with -1 (zero indexed)
     *  4. Limit with the page size
     *  5. Sort ascending on PokemonId
     *  Might be better performance with a range query, but then we need the boundary value.
     * </summary>
     */
    public async Task<PaginationResultDocument> FindByPaginationAsync(
        int page,
        int pageSize,
        List<PokemonType> types,
        List<PokemonSpecial> specials,
        CancellationToken cancellationToken = default)
    {
        var builder = Filter;
        var filter = builder.Empty;
        filter &= builder.And(GetTypeFilters(types));
        filter &= builder.And(GetSpecialFilters(specials));
        var count = await _collection.CountDocumentsAsync(
            filter,
            null,
            cancellationToken);
        var totalPages = (int)Math.Ceiling((double)count / pageSize);
        var documents = await _collection
            .Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .Sort(Sort.Ascending(p => p.Id))
            .ToListAsync(cancellationToken);

        return new PaginationResultDocument
        {
            TotalCount = count,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            Documents = documents
        };
    }

    private static List<FilterDefinition<PokemonSpeciesDocument>> GetSpecialFilters(List<PokemonSpecial> specials)
    {
        return specials.Select(special =>
        {
            if (special == PokemonSpecial.Legendary)
            {
                return Filter.Eq(p => p.Special.Legendary, true);
            }
            else if (special == PokemonSpecial.Baby)
            {
                return Filter.Eq(p => p.Special.Baby, true);
            }
            else
            {
                return Filter.Eq(p => p.Special.Mythical, true);
            }
        }).ToList();
    }

    private static List<FilterDefinition<PokemonSpeciesDocument>> GetTypeFilters(List<PokemonType> types)
    {
        return types.Select(type =>
        {
            return Filter.ElemMatch(
                doc => doc.Varieties,
                variety => variety.Types.Any(document => document.Type == type.Value) && variety.Default);
        }).ToList();
    }
}