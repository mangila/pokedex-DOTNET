using System.Net.Mime;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Bson;
using pokedex_api.Config;
using pokedex_shared.Model.Domain;
using pokedex_shared.Service;

namespace pokedex_api.Controller;

[ApiController]
[Route("api/v1/pokemon")]
[Produces(MediaTypeNames.Application.Json)]
[EnableRateLimiting(HttpRateLimiterConfig.Policies.FixedWindow)]
public class PokemonV1Controller(
    ILogger<PokemonV1Controller> logger,
    PokemonService pokemonService)
    : ControllerBase
{
    [HttpGet]
    [RequestTimeout(HttpRequestConfig.Policies.OneMinute)]
    public async Task<IResult> FindAll(CancellationToken cancellationToken = default)
    {
        var collection = await pokemonService.FindAllAsync(cancellationToken);
        return Results.Ok(collection);
    }

    [HttpGet("{id:int}")]
    [RequestTimeout(HttpRequestConfig.Policies.OneMinute)]
    public async Task<IResult> FindByPokemonId(
        [FromRoute] int id,
        CancellationToken cancellationToken = default
    )
    {
        var dto = await pokemonService.FindOneByPokemonIdAsync(new PokemonId(id.ToString()), cancellationToken);
        return dto.HasValue ? Results.Ok(dto) : Results.NotFound();
    }

    [HttpGet("{name}")]
    [RequestTimeout(HttpRequestConfig.Policies.OneMinute)]
    public async Task<IResult> FindByPokemonName(
        [FromRoute] string name,
        CancellationToken cancellationToken = default
    )
    {
        var dto = await pokemonService.FindOneByNameAsync(new PokemonName(name), cancellationToken);
        return dto.HasValue ? Results.Ok(dto) : Results.NotFound();
    }

    [HttpGet("search/id")]
    [RequestTimeout(HttpRequestConfig.Policies.OneMinute)]
    public async Task<IResult> QueryById(
        [FromQuery] List<int> ids,
        CancellationToken cancellationToken = default
    )
    {
        var collection = await pokemonService.FindAllByPokemonIdAsync(
            new PokemonIdCollection(ids),
            cancellationToken);
        return Results.Ok(collection);
    }

    [HttpGet("search/name")]
    [RequestTimeout(HttpRequestConfig.Policies.OneMinute)]
    public async Task<IResult> QueryByName(
        [FromQuery] string search,
        CancellationToken cancellationToken = default
    )
    {
        var collection = await pokemonService.SearchByNameAsync(new PokemonName(search), cancellationToken);
        return Results.Ok(collection);
    }

    [HttpGet("file/{id}")]
    [RequestTimeout(HttpRequestConfig.Policies.OneMinute)]
    public async Task<IResult> FindFileById(
        [FromRoute] string id,
        CancellationToken cancellationToken = default
    )
    {
        var result = await pokemonService.FindFileByIdAsync(new ObjectId(id), cancellationToken);
        return result is not null ? Results.File(result.File, result.ContentType, result.FileName) : Results.NotFound();
    }
}