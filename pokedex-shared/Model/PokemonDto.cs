﻿using System.Text.Json.Serialization;
using pokedex_shared.Extension;

namespace pokedex_shared.Model;

public readonly record struct PokemonDto(
    [property: JsonPropertyName("pokemon_id")]
    string PokemonId,
    [property: JsonPropertyName("name")] string Name
);

public static partial class Extensions
{
    public static PokemonDocument ToDocument(this PokemonDto dto)
    {
        var document = new PokemonDocument
        {
            PokemonId = dto.PokemonId,
            Name = dto.Name
        };
        document.Validate();
        return document;
    }
}