﻿using System.ComponentModel.DataAnnotations;
using pokedex_shared.Common;

namespace pokedex_shared.Model.Domain;

public readonly record struct PokemonId
{
    [Required]
    [StringLength(5, ErrorMessage = "length cannot be over 5")]
    [RegularExpression("[\\d]+", ErrorMessage = "must be a number")]
    public string Value { get; }

    public PokemonId(string value)
    {
        Value = value;
        this.ValidateValueType();
    }

    public PokemonId(int value)
    {
        Value = value.ToString();
        this.ValidateValueType();
    }

    public int ToInt()
    {
        return Convert.ToInt32(Value);
    }
}