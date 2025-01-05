﻿# Pokedex

A pretty Pokédex (yet another)

## pokedex-ui

Blazor project that displays the Pokemon data. Using Bootstrap components.
* http://localhost:5122/

## pokedex-api

Restful API with swagger endpoint. Connected to a Redis docker instance and a MongoDB docker instance 
for a cache-a-side pattern
* http://localhost:5144/swagger

## pokedex-poller

C# Worker Service project that polls data from PokeApi and persists the fetched data to a MongoDB docker instance.

## datasource
MongoDB as database and Redis as a IDistributedCache.
* Redis Insight - http://localhost:8001
* Mongo Express - http://localhost:8081


