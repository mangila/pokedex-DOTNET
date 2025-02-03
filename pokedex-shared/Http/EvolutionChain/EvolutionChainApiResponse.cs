﻿using System.Text.Json.Serialization;

namespace pokedex_shared.Http.EvolutionChain;

public readonly record struct EvolutionChainApiResponse(Chain Chain);

public readonly record struct Chain(
    [property: JsonPropertyName("evolves_to")]
    EvolutionChain[] FirstChain,
    [property: JsonPropertyName("species")]
    Species Species
);

public readonly record struct EvolutionChain(
    [property: JsonPropertyName("evolves_to")]
    EvolutionChain[] NextChain,
    [property: JsonPropertyName("species")]
    Species Species
);

public readonly record struct Species(
    [property: JsonPropertyName("name")] string Name
);