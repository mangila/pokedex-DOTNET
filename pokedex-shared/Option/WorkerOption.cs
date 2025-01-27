﻿using System.ComponentModel.DataAnnotations;

namespace pokedex_shared.Option;

public class WorkerOption
{
    [Required] public required Interval Interval { get; set; }
}

public class Interval
{
    [Required] public required int Min { get; init; }
    [Required] public required int Max { get; init; }
}